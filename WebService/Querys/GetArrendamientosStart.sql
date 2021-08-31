select
	CASE WHEN IND.moralphy = 'P' THEN IND.PRENOM || ' ' || IND.NOMCOMPL || ' ' ELSE '' END  || IND.NOM || CASE WHEN IND.moralphy = 'P' THEN ' ' || IND.STR33 ELSE '' END || ' ' || CASE WHEN IND.moralphy = 'M' THEN IND.GENRE ELSE '' END Cliente,
	IND.TVA RFC,
    tabla.Rentas_Devengar * (1 + tabla.TASA_IVA) Rentas_Devengar,
    (tabla.RENTAS_DEVENGAR + ta.RESIDUAL_VALUE) * (1 + tabla.TASA_IVA) Rentas_Devengar_Residual,
    (tabla.SALDO_INS_CAPITAL + ta.RESIDUAL_VALUE) * (1 + tabla.TASA_IVA) Riesgo_Expuesto,
    NVL(OCon.Otros_Conceptos * (1 + tabla.TASA_IVA), 0) Otros_Conceptos_Vencidos,
    NVL(Venc.Rentas_Vencidas * (1 + tabla.TASA_IVA), 0) Total_Exigible
	from imxdb.G_DOSSIER DOSS
		inner join (
            Select REFTYPE, REFDOSS, REFINDIVIDU
            From imxdb.T_INTERVENANTS
            Where NVL(reftype,'DB') In ('DB')
			) T on t.refdoss = DOSS.refdoss
		inner join (
            Select MORALPHY, PRENOM, NOMCOMPL, NOM, STR33, GENRE, TVA, REFINDIVIDU
            From imxdb.G_INDIVIDU) IND on t.refindividu = IND.refindividu
		inner join (
            select c.refdoss_reqst, max(c.imx_un_id) imx_un_id
			from imxdb.T_FIN_AMO c inner join (
                Select IMX_UN_ID, TYPE, STR3, DT04_DT From IMXDB.g_piecedet Where TYPE = 'ANNEXE_DEMANDE_FIN' AND STR3 = 'ACT')det on det.imx_un_id = c.ANNEX_ID
			where det.DT04_DT < (TRUNC(sysdate,'DD')+1)
			and (flag_active is null or flag_active = 'O')
		group by c.refdoss_reqst) uvt on uvt.refdoss_reqst = DOSS.refdoss
		inner join (
            Select IMX_UN_ID, REFDOSS_REQST, ANNEX_ID, RESIDUAL_VALUE
            From imxdb.T_FIN_AMO) ta on ta.refdoss_reqst = DOSS.refdoss and ta.imx_un_id = uvt.imx_un_id
		inner join (
			select distinct his.fin_amort_id
			, SUM(case when his.FEC_FACTURADO >= (TRUNC(sysdate,'DD')+1) THEN ROUND(his.Capital,2) ELSE 0 END) SALDO_INS_CAPITAL
			, count(his.instal_number) PLAZO
			, MIN(his.instal_due_dt) VEN_PRIMER_RENTA
			, SUM(case when his.FEC_FACTURADO >= (TRUNC(sysdate,'DD')+1) THEN ROUND((his.Capital+his.Interes),2) ELSE 0 END) RENTAS_DEVENGAR
            , MAX(his.DF_TAUX_TVA) TASA_IVA
	from (
		select amort.fin_amort_id, amort.instal_due_dt, amort.instal_number, amort.capital_installment Capital, amort.interest_installment interes, nvl(ent.ER_DAT_DT,amort.instal_due_dt) FEC_FACTURADO
            , CASE WHEN ent.DF_TAUX_TVA = 0 THEN 0.16 ELSE NVL(ent.DF_TAUX_TVA, 0.16) END DF_TAUX_TVA
		from imxdb.t_amort_histo amort 
		left join 
			(select elem.refelem, entx.er_dat_dt, det.DF_TAUX_TVA from imxdb.g_elemfi elem 
			inner join (select d.df_num, d.df_rel, d.DF_TAUX_TVA from imxdb.f_detfac d) det on det.df_num = elem.LIBELLE_20_2 
			inner join (select e.er_num, e.ER_DAT_DT from imxdb.f_entrel e) entx on entx.er_num = det.df_rel 
			where elem.libelle_20_3 = 'LOY'
			) ent on ent.refelem = amort.refelem_fi_inst
		union all
		select amort.fin_amort_id, amort.instal_due_dt, amort.instal_number, amort.capital_installment Capital, amort.interest_installment interes, nvl(ent.ER_DAT_DT,amort.instal_due_dt) FEC_FACTURADO
            , CASE WHEN ent.DF_TAUX_TVA = 0 THEN 0.16 ELSE NVL(ent.DF_TAUX_TVA, 0.16) END DF_TAUX_TVA
		from imxdb.T_AMORT_INSTAL amort 
		left join 
			(select elem.refelem, entx.er_dat_dt, det.DF_TAUX_TVA from imxdb.g_elemfi elem 
			inner join (select d.df_num, d.df_rel, d.DF_TAUX_TVA from imxdb.f_detfac d) det on det.df_num = elem.LIBELLE_20_2 
			inner join (select e.er_num, e.ER_DAT_DT from imxdb.f_entrel e) entx on entx.er_num = det.df_rel 
			where elem.libelle_20_3 = 'LOY'
			) ent on ent.refelem = amort.refelem_fi_inst
		) his 
		group by his.fin_amort_id  
	) tabla on tabla.fin_amort_id = uvt.imx_un_id
    Left Join (
        (Select df_dos, Rentas_Vencidas, Saldo_Vencido_Capital, ER_REG_DT from
        (Select df_dos
            , F.ER_TDB Rentas_Vencidas
            , F.ER_TDB Saldo_Vencido_Capital        
            ,  er_reg_dt ER_REG_DT
        from imxdb.F_ENTREL F
        inner join (
            Select LIBELLE
            From imxdb.G_ELEMFI) g on f.er_refext1 = g.libelle
        inner join (
            Select DF_REL, DF_NOM, DF_DOS, DF_INV_GROUP
            From imxdb.F_DETFAC) d on f.er_num = d.df_rel
        where F.ER_TDB_MVT > NVL(F.ER_PAYE_MVT, 0)
            and upper(d.df_nom) = 'LOY'
        order by er_reg_dt) where rownum = 1)
    ) Venc On Venc.df_dos = DOSS.REFDOSS
    Left Join (
        (Select df_dos, Otros_Conceptos from
        (Select df_dos
            , F.ER_TDB Otros_Conceptos    
        from imxdb.F_ENTREL F
        inner join (
            Select LIBELLE
            From imxdb.G_ELEMFI) g on f.er_refext1 = g.libelle
        inner join (
            Select DF_REL, DF_NOM, DF_DOS, DF_INV_GROUP
            From imxdb.F_DETFAC) d on f.er_num = d.df_rel
        where F.ER_TDB_MVT > NVL(F.ER_PAYE_MVT, 0)
            and d.DF_INV_GROUP = 'C'
        order by er_reg_dt) where rownum = 1)
    ) OCon On OCon.df_dos = DOSS.REFDOSS
where doss.categdoss LIKE 'FINANCING REQUEST%'
And DOSS.ANCREFDOSS = '