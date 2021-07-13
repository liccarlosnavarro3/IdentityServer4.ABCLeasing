select ROW_NUMBER() OVER (ORDER BY DOSS.ancrefdoss) NB 
    , CASE WHEN IND.moralphy = 'P' THEN IND.PRENOM || ' ' || IND.NOMCOMPL || ' ' ELSE '' END  || IND.NOM || CASE WHEN IND.moralphy = 'P' THEN ' ' || IND.STR33 ELSE '' END || ' ' || CASE WHEN IND.moralphy = 'M' THEN IND.GENRE ELSE '' END CLIENTE
    , IND.TVA RFC
    , DOSS.ANCREFDOSS CONTRATO    
    , tabla.*
from imxdb.G_DOSSIER DOSS
Inner join (Select REFDOSS, typpiece
    From imxdb.G_PIECE) P on DOSS.REFDOSS = P.REFDOSS and p.typpiece = 'FINANCING REQUEST'
left join (Select refdoss, reftype, refindividu
    From imxdb.T_INTERVENANTS) T on t.refdoss = DOSS.refdoss and T.reftype = 'DB'
left join (Select moralphy, PRENOM, NOMCOMPL, NOM, STR33, GENRE, TVA, refindividu
    From imxdb.G_INDIVIDU) IND on t.refindividu = IND.refindividu 
inner join (
        select det.refdoss, det.STR1 ESTATUS , det.DT01_DT FEC_ESTATUS from imxdb.g_piecedet det,
            ( select distinct PP.Refdoss,  max(PP.imx_un_id) imx_un_id
                    from imxdb.g_piecedet PP where PP.TYPE = 'REQUEST SITUATIONS' AND PP.DT01_DT < (trunc(sysdate) - (to_number(to_char(sysdate,'DD')) - 1)) group by PP.refdoss ------ Fecha al corte + 1 día 
                    ) det_max
        where det.imx_un_id = det_max.imx_un_id
        and det.refdoss = det_max.refdoss and STR1 in ('ACT', 'ECT', 'SIG')
        ) FR_ACTIVAS_AL_CORTE on FR_ACTIVAS_AL_CORTE.refdoss = DOSS.refdoss
-------------- GGF  CONTIENE EL ID DE LA AMORTIZACION ACTIVA A LA FECHA REQUERIDA
inner join (select c.refdoss_reqst,  max(c.imx_un_id) imx_un_id
           from imxdb.T_FIN_AMO c inner join imxdb.g_piecedet det on det.imx_un_id = c.ANNEX_ID AND det.TYPE = 'ANNEXE_DEMANDE_FIN' AND det.STR3 = 'ACT' 
          where det.DT04_DT < (trunc(sysdate) - (to_number(to_char(sysdate,'DD')) - 1)) ------ Fecha al corte + 1 día
            and (flag_active is null or flag_active = 'O')
    group by c.refdoss_reqst) uvt on uvt.refdoss_reqst = DOSS.refdoss
----------------- GGF 2020-04-29 tablas amortizacion; incluye fecha de facturación para discriminar las facturas anticipadas
inner join (
Select distinct his.fin_amort_id
    , sum (case when his.FEC_FACTURADO < TO_DATE('01/01/19','DD/MM/YY') THEN ROUND(((his.Capital+Interes)),2) ELSE 0 END) RENTAS_MENORES_2018
    , sum (case when his.INSTAL_YEAR = 2019 and (his.INSTAL_MONTH = 1 Or his.INSTAL_MONTH = 2 Or his.INSTAL_MONTH = 3 Or his.INSTAL_MONTH = 4 Or his.INSTAL_MONTH = 5 Or his.INSTAL_MONTH = 6) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_1_2019
    , sum (case when his.INSTAL_YEAR = 2019 and (his.INSTAL_MONTH = 7 Or his.INSTAL_MONTH = 8 Or his.INSTAL_MONTH = 9 Or his.INSTAL_MONTH = 10 Or his.INSTAL_MONTH = 11 Or his.INSTAL_MONTH = 12) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_2_2019
    , sum (case when his.INSTAL_YEAR = 2020 and (his.INSTAL_MONTH = 1 Or his.INSTAL_MONTH = 2 Or his.INSTAL_MONTH = 3 Or his.INSTAL_MONTH = 4 Or his.INSTAL_MONTH = 5 Or his.INSTAL_MONTH = 6) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_1_2020
    , sum (case when his.INSTAL_YEAR = 2020 and (his.INSTAL_MONTH = 7 Or his.INSTAL_MONTH = 8 Or his.INSTAL_MONTH = 9 Or his.INSTAL_MONTH = 10 Or his.INSTAL_MONTH = 11 Or his.INSTAL_MONTH = 12) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_2_2020
    , sum (case when his.INSTAL_YEAR = 2021 and (his.INSTAL_MONTH = 1 Or his.INSTAL_MONTH = 2 Or his.INSTAL_MONTH = 3 Or his.INSTAL_MONTH = 4 Or his.INSTAL_MONTH = 5 Or his.INSTAL_MONTH = 6) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_1_2021
    , sum (case when his.INSTAL_YEAR = 2021 and (his.INSTAL_MONTH = 7 Or his.INSTAL_MONTH = 8 Or his.INSTAL_MONTH = 9 Or his.INSTAL_MONTH = 10 Or his.INSTAL_MONTH = 11 Or his.INSTAL_MONTH = 12) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_2_2021
    , sum (case when his.INSTAL_YEAR = 2022 and (his.INSTAL_MONTH = 1 Or his.INSTAL_MONTH = 2 Or his.INSTAL_MONTH = 3 Or his.INSTAL_MONTH = 4 Or his.INSTAL_MONTH = 5 Or his.INSTAL_MONTH = 6) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_1_2022
    , sum (case when his.INSTAL_YEAR = 2022 and (his.INSTAL_MONTH = 7 Or his.INSTAL_MONTH = 8 Or his.INSTAL_MONTH = 9 Or his.INSTAL_MONTH = 10 Or his.INSTAL_MONTH = 11 Or his.INSTAL_MONTH = 12) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_2_2022
    , sum (case when his.INSTAL_YEAR = 2023 and (his.INSTAL_MONTH = 1 Or his.INSTAL_MONTH = 2 Or his.INSTAL_MONTH = 3 Or his.INSTAL_MONTH = 4 Or his.INSTAL_MONTH = 5 Or his.INSTAL_MONTH = 6) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_1_2023
    , sum (case when his.INSTAL_YEAR = 2023 and (his.INSTAL_MONTH = 7 Or his.INSTAL_MONTH = 8 Or his.INSTAL_MONTH = 9 Or his.INSTAL_MONTH = 10 Or his.INSTAL_MONTH = 11 Or his.INSTAL_MONTH = 12) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_2_2023
    , sum (case when his.INSTAL_YEAR = 2024 and (his.INSTAL_MONTH = 1 Or his.INSTAL_MONTH = 2 Or his.INSTAL_MONTH = 3 Or his.INSTAL_MONTH = 4 Or his.INSTAL_MONTH = 5 Or his.INSTAL_MONTH = 6) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_1_2024
    , sum (case when his.INSTAL_YEAR = 2024 and (his.INSTAL_MONTH = 7 Or his.INSTAL_MONTH = 8 Or his.INSTAL_MONTH = 9 Or his.INSTAL_MONTH = 10 Or his.INSTAL_MONTH = 11 Or his.INSTAL_MONTH = 12) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_2_2024
    , sum (case when his.INSTAL_YEAR = 2025 and (his.INSTAL_MONTH = 1 Or his.INSTAL_MONTH = 2 Or his.INSTAL_MONTH = 3 Or his.INSTAL_MONTH = 4 Or his.INSTAL_MONTH = 5 Or his.INSTAL_MONTH = 6) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_1_2025
    , sum (case when his.INSTAL_YEAR = 2025 and (his.INSTAL_MONTH = 7 Or his.INSTAL_MONTH = 8 Or his.INSTAL_MONTH = 9 Or his.INSTAL_MONTH = 10 Or his.INSTAL_MONTH = 11 Or his.INSTAL_MONTH = 12) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_2_2025
    , sum (case when his.INSTAL_YEAR = 2026 and (his.INSTAL_MONTH = 1 Or his.INSTAL_MONTH = 2 Or his.INSTAL_MONTH = 3 Or his.INSTAL_MONTH = 4 Or his.INSTAL_MONTH = 5 Or his.INSTAL_MONTH = 6) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_1_2026
    , sum (case when his.INSTAL_YEAR = 2026 and (his.INSTAL_MONTH = 7 Or his.INSTAL_MONTH = 8 Or his.INSTAL_MONTH = 9 Or his.INSTAL_MONTH = 10 Or his.INSTAL_MONTH = 11 Or his.INSTAL_MONTH = 12) THEN ROUND((his.Capital+Interes),2) ELSE 0 END) RENTAS_SEMESTRE_2_2026
from (
    ------------- TABLA HISTÓRICA DE ANEXOS ANTERIORES
            select amort.fin_amort_id, amort.leas_end_date, amort.instal_due_dt, amort.instal_number, amort.capital_installment Capital, amort.interest_installment interes, INSTAL_VAT_CATEG, nvl(ent.ER_DAT_DT,amort.instal_due_dt) FEC_FACTURADO
                , EXTRACT(YEAR FROM nvl(ent.ER_DAT_DT,amort.instal_due_dt)) INSTAL_YEAR, EXTRACT( MONTH FROM nvl(ent.ER_DAT_DT,amort.instal_due_dt) ) INSTAL_MONTH, 'REN' TIPO
               from imxdb.t_amort_histo amort 
                       left join 
                            ( select elem.refelem, case when det.df_inv_group = 'R' then trunc(sysdate+1,'DD') else entx.er_dat_dt end er_dat_dt from imxdb.g_elemfi elem 
                                                inner join (select d.df_num, d.df_rel, d.df_inv_group from imxdb.f_detfac d) det on det.df_num = elem.LIBELLE_20_2 
                                                inner join (select e.er_num, e.ER_DAT_DT from imxdb.f_entrel e) entx on entx.er_num = det.df_rel 
                                where elem.libelle_20_3 = 'LOY'
                            ) ent on ent.refelem = amort.refelem_fi_inst
        union all
    ---------- TABLA ACTIVA ACTUAL    
            select amort.fin_amort_id, amort.leas_end_date, amort.instal_due_dt, amort.instal_number, amort.capital_installment Capital, amort.interest_installment interes, INSTAL_VAT_CATEG, nvl(ent.ER_DAT_DT,amort.instal_due_dt) FEC_FACTURADO
                , EXTRACT(YEAR FROM nvl(ent.ER_DAT_DT,amort.instal_due_dt)) INSTAL_YEAR, EXTRACT( MONTH FROM nvl(ent.ER_DAT_DT,amort.instal_due_dt) ) INSTAL_MONTH, 'REN' TIPO
            from imxdb.T_AMORT_INSTAL amort 
                     left join 
                            ( select elem.refelem, case when det.df_inv_group = 'R' then trunc(sysdate+1,'DD') else entx.er_dat_dt end er_dat_dt from imxdb.g_elemfi elem 
                                                inner join (select d.df_num, d.df_rel, d.df_inv_group from imxdb.f_detfac d) det on det.df_num = elem.LIBELLE_20_2 
                                                inner join (select e.er_num, e.ER_DAT_DT from imxdb.f_entrel e) entx on entx.er_num = det.df_rel 
                                where elem.libelle_20_3 = 'LOY'
                            ) ent on ent.refelem = amort.refelem_fi_inst
            
        union all         
    --------- PROYECCIÓN DEL RESIDUAL PARA TABLA ACTIVA Y TABLAS HISTÓRICAS ANTERIOES    
            select ta.imx_un_id, TA.leas_end_date
                , (case when TA.leas_end_date < sysdate then TRUNC(SYSDATE + 1, 'DD') else (TA.leas_end_date + INTERVAL '1' DAY) end) instal_due_dt
                , CEIL(MONTHS_BETWEEN(TA.LEAS_END_DATE, TA.LEAS_START_DATE)) + 1 INSTALL_NUMBER, ta.RESIDUAL_VALUE Capital, 0 interes, 'TVA1' INSTAL_VA_CATEG
                , nvl(ent.ER_DAT_DT,(case when TA.leas_end_date < sysdate then TRUNC(SYSDATE + 1, 'DD') else (TA.leas_end_date + INTERVAL '1' DAY) end)) FEC_FACTURADO
                , EXTRACT(YEAR FROM nvl(ent.ER_DAT_DT,(case when TA.leas_end_date < sysdate then TRUNC(SYSDATE + 1, 'DD') else (TA.leas_end_date + INTERVAL '1' DAY) end))) INSTAL_YEAR
                , EXTRACT( MONTH FROM nvl(ent.ER_DAT_DT,(case when TA.leas_end_date < sysdate then TRUNC(SYSDATE + 1, 'DD') else (TA.leas_end_date + INTERVAL '1' DAY) end) ) ) INSTAL_MONTH
                , 'RES' TIPO                    
            from imxdb.T_FIN_AMO ta 
                left join           
                            ( select distinct entx.er_dat_dt, elem.LIBELLE_20_6 from imxdb.g_elemfi elem 
                                                inner join (select d.df_num, d.df_rel from imxdb.f_detfac d ) det on det.df_num = elem.LIBELLE_20_2 
                                                inner join (select e.er_num, e.ER_DAT_DT from imxdb.f_entrel e) entx on entx.er_num = det.df_rel 
                                where elem.libelle_20_3 = 'RABC'
                            ) ent ON ent.LIBELLE_20_6 = to_char(ta.annex_id)

    ) his 
    
group by his.fin_amort_id


) tabla on tabla.fin_amort_id = uvt.imx_un_id

where doss.categdoss LIKE 'FINANCING REQUEST%' 
And IND.TVA = '