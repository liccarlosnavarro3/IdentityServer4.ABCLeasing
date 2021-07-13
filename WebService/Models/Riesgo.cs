using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService.Models
{
    public class Riesgo
    {
        public string cliente { get; set; }
        public string rfc { get; set; }
        public double rentas_devengar { get; set; }
        public double rentas_devengar_residual { get; set; }
        public double riesgo_expuesto { get; set; }
        public double otros_conceptos_vencidos { get; set; }
        public double total_exigible { get; set; }
    }

    public class ClienteImx
    {
            public string rfc { get; set; }
            public string email_notificacion { get; set; }
            public string email_facturacion { get; set; }
            public string telefono { get; set; }
            public string celular { get; set; }
    }

    public class ContratoConcentrado
    {
        public string contrato { get; set; }
        public int periodo01 { get; set; }
        public int periodo02 { get; set; }
        public int periodo03 { get; set; }
        public int periodo04 { get; set; }
        public int periodo05 { get; set; }
        public int periodo06 { get; set; }
        public int periodo07 { get; set; }
        public int periodo08 { get; set; }
        public int periodo09 { get; set; }
        public int periodo10 { get; set; }
        public int periodo11 { get; set; }
        public int periodo12 { get; set; }
        public int periodo13 { get; set; }
        public int periodo14 { get; set; }
        public int periodo15 { get; set; }
        public int periodo16 { get; set; }
        public int periodo17 { get; set; }
    }

    public class ContratoDetalle
    {
        public string contrato { get; set; }
        public double importe_factura { get; set; }
        public double importe_pagado { get; set; }
        public double dias_atraso { get; set; }
        public string concepto { get; set; }
        public string periodo { get; set; }
    }

    public class ClabeSTP
    {
        public string clabe { get; set; }
    }

    public class EdoCuenta
    {
        public string solicitud_financiacion { get; set; }
        public string fecha_ejecucion { get; set; }
    }
}