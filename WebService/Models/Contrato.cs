using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService.Models
{
    public class Contrato
    {
        public String rfc_user { get; set; }
        public String tipo_relacion { get; set; }
        public String contrato { get; set; }
        public double monto_financiado { get; set; }
        public double residual_iva { get; set; }
        public double saldo_ins_inv_neta { get; set; }
        public double importe_rentas_vencidas { get; set; }
        public double otros_conceptos_vencidos { get; set; }
        public double intereses_moratorios { get; set; }
        public String fecha_activacion { get; set; }
        public double rentas_devengar { get; set; }
        public int plazo { get; set; }
        public int meses_pagados { get; set; }
        public double saldo_vigente_capital { get; set; }
        public String proxima_renta { get; set; }
        public int num_rentas_vencidas { get; set; }
    }
}