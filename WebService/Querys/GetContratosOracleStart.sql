select DOSS.ANCREFDOSS Contrato, to_char(p.DT02_DT, 'dd/mm/yyyy')  Fecha_Activacion, IND.TVA RFC, T.reftype Tipo_Relacion, ROUND(nvl(p.MT10,0),2) Monto_Financiado, ROUND(ta.RESIDUAL_VALUE * TC.DBL_TC,2) Residual_Sin_Iva,
    ROUND((tabla.SALDO_INS_CAPITAL + ta.RESIDUAL_VALUE) * TC.DBL_TC,2) Saldo_Ins_Inv_Neta, NVL(Venc.Rentas_Vencidas, 0) Importe_Rentas_Vencidas, NVL(Venc.Otros_Conceptos, 0) Otros_Conceptos_Vencidos,
    NVL(Venc.Intereses_Moratorios, 0) Intereses_Moratorios, tabla.Rentas_Devengar, tabla.PLAZO Plazo,
	NVL((Round(MONTHS_BETWEEN(sysdate,tabla.VEN_PRIMER_RENTA),0)+1)-(Case When TC.FEC_CORTE - Venc.ER_REG_DT < 0 then 0 Else Round((TC.FEC_CORTE - Venc.ER_REG_DT)/30, 0) End), 0) Meses_Pagados,
	tabla.RENTAS_DEVENGAR Saldo_Vigente_Capital,
    NVL((Select max(to_char(t_i.INSTAL_DUE_DT, 'dd/mm/yyyy')) FROM IMXDB.T_FIN_AMO t_a Inner Join (
        Select INSTAL_DUE_DT, FIN_AMORT_ID From IMXDB.T_AMORT_INSTAL) t_i On t_i.FIN_AMORT_ID = t_a.imx_un_id Where t_i.FIN_AMORT_ID = uvt.imx_un_id And to_char(t_i.INSTAL_DUE_DT, 'dd/mm/yyyy') Between to_char(ADD_MONTHS(TRUNC(SYSDATE,'MM'),+1), 'dd/mm/yyyy') And to_char(ADD_MONTHS(LAST_DAY(TRUNC(SYSDATE)),+1), 'dd/mm/yyyy')),'01/01/1990') PROXIMA_RENTA,
    NVL(Case When TC.FEC_CORTE - Venc.ER_REG_DT < 0 Then 0 Else Round((TC.FEC_CORTE - Venc.ER_REG_DT)/30, 0) End, 0) NUM_RENTAS_VENCIDAS
from imxdb.G_DOSSIER DOSS
	inner join (Select DT02_DT, MT10, TX02, REFDOSS, TYPPIECE From imxdb.G_PIECE Where typpiece = 'FINANCING REQUEST') P on DOSS.REFDOSS = P.REFDOSS
    inner join (Select REFDOSS, ANCREFDOSS, DEVISE, REFLOT, CATEGDOSS From imxdb.G_DOSSIER) DF on DOSS.reflot = DF.refdoss
	inner join (Select REFTYPE, REFDOSS, REFINDIVIDU From imxdb.T_INTERVENANTS Where NVL(reftype,'DB') = 'DB') T on t.refdoss = DOSS.refdoss
	inner join (Select MORALPHY, PRENOM, NOMCOMPL, NOM, STR33, GENRE, TVA, DIVISION, STR44, REFINDIVIDU, ADR2 From imxdb.G_INDIVIDU) IND on t.refindividu = IND.refindividu
	left join (Select REFTYPE, REFDOSS, REFINDIVIDU From imxdb.T_INTERVENANTS Where NVL(reftype,'DB') = 'EPB') PROMOTOR_T on PROMOTOR_T.refdoss = DOSS.refdoss
    inner join (
        select c.refdoss_reqst, max(c.imx_un_id) imx_un_id
        from imxdb.T_FIN_AMO c inner join (Select IMX_UN_ID, TYPE, STR3, DT04_DT From IMXDB.g_piecedet Where TYPE = 'ANNEXE_DEMANDE_FIN' AND STR3 = 'ACT')det on det.imx_un_id = c.ANNEX_ID
        where det.DT04_DT < (TRUNC(sysdate,'DD')+1) and (flag_active is null or flag_active = 'O')
        group by c.refdoss_reqst) uvt on uvt.refdoss_reqst = DOSS.refdoss
    inner join (Select IMX_UN_ID, REFDOSS_REQST, ANNEX_ID, RESIDUAL_VALUE From imxdb.T_FIN_AMO) ta on ta.refdoss_reqst = DOSS.refdoss and ta.imx_un_id = uvt.imx_un_id
    inner join (
        select distinct his.fin_amort_id, SUM(case when his.FEC_FACTURADO >= (TRUNC(sysdate,'DD')+1) THEN ROUND(his.Capital,2) ELSE 0 END) SALDO_INS_CAPITAL, count(his.instal_number) PLAZO
            , MIN(his.instal_due_dt) VEN_PRIMER_RENTA, SUM(case when his.FEC_FACTURADO >= (TRUNC(sysdate,'DD')+1) THEN ROUND((his.Capital+his.Interes),2) ELSE 0 END) RENTAS_DEVENGAR
        from (
            select amort.fin_amort_id, amort.instal_due_dt, amort.instal_number, amort.capital_installment Capital, amort.interest_installment interes, nvl(ent.ER_DAT_DT,amort.instal_due_dt) FEC_FACTURADO
            from imxdb.t_amort_histo amort 
            left join 
            (select elem.refelem, entx.er_dat_dt from imxdb.g_elemfi elem 
                inner join (select d.df_num, d.df_rel from imxdb.f_detfac d) det on det.df_num = elem.LIBELLE_20_2 
                inner join (select e.er_num, e.ER_DAT_DT from imxdb.f_entrel e) entx on entx.er_num = det.df_rel 
            where elem.libelle_20_3 = 'LOY'
            ) ent on ent.refelem = amort.refelem_fi_inst
        union all
            select amort.fin_amort_id, amort.instal_due_dt, amort.instal_number, amort.capital_installment Capital, amort.interest_installment interes, nvl(ent.ER_DAT_DT,amort.instal_due_dt) FEC_FACTURADO
            from imxdb.T_AMORT_INSTAL amort 
            left join 
            (select elem.refelem, entx.er_dat_dt from imxdb.g_elemfi elem 
                inner join (select d.df_num, d.df_rel from imxdb.f_detfac d) det on det.df_num = elem.LIBELLE_20_2 
                inner join (select e.er_num, e.ER_DAT_DT from imxdb.f_entrel e) entx on entx.er_num = det.df_rel 
            where elem.libelle_20_3 = 'LOY'
            ) ent on ent.refelem = amort.refelem_fi_inst
        ) his 
        group by his.fin_amort_id  
    ) tabla on tabla.fin_amort_id = uvt.imx_un_id
	inner join (
        select TTC.origine TXT_MONEDA, round(1/NVL(TTC."TAUX", 1.0),4) as DBL_TC, dtdebut_dt FEC_TC, FEC_CORTE
        from imxdb.t_devise TTC, (
            select origine, max(dtdebut_dt) last_dt , FEC_CORTE
            from imxdb.t_devise , (
                select trunc(sysdate) - (to_number(to_char(sysdate,'DD')) - 1) - 1 FEC_CORTE from dual) PARAMS where type = 'MR' and PLACE = 'MEX' and dtdebut_dt <= PARAMS.FEC_CORTE
            group by origine) MAX_TC
        where TTC.origine = MAX_TC.origine and TTC.dtdebut_dt = MAX_TC.last_dt 
	) TC on TC.TXT_MONEDA =  DOSS.devise
    Inner Join (
        Select Distinct df_dos, Case When upper(d.df_nom) = 'LOY' Then F.ER_TDB Else 0 End Rentas_Vencidas, Case When d.DF_INV_GROUP = 'C' Then F.ER_TDB Else 0 End Otros_Conceptos
            , Case When upper(d.df_nom) = 'LPI' Then F.ER_TDB Else 0 End Intereses_Moratorios, Case When upper(d.df_nom) = 'LOY' Then F.ER_TDB Else 0 End Saldo_Vencido_Capital, Case When upper(d.df_nom) = 'LOY' Then er_reg_dt End ER_REG_DT
        from imxdb.F_ENTREL F
            inner join (Select LIBELLE From imxdb.G_ELEMFI) g on f.er_refext1 = g.libelle
            inner join (Select DF_REL, DF_NOM, DF_DOS, DF_INV_GROUP From imxdb.F_DETFAC) d on f.er_num = d.df_rel
        where F.ER_TDB_MVT > NVL(F.ER_PAYE_MVT, 0) and (upper(d.df_nom) = 'LOY' Or d.DF_INV_GROUP = 'C' Or upper(d.df_nom) = 'LPI')
        order by er_reg_dt
    ) Venc On Venc.df_dos = DOSS.REFDOSS	
where doss.categdoss LIKE 'FINANCING REQUEST%' And IND.TVA = '