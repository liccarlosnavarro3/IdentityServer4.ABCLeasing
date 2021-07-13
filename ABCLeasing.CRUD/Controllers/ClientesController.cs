using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ABCLeasing.CRUD;
using PagedList;

namespace ABCLeasing.CRUD.Controllers
{
    public class ClientesController : Controller
    {
        private ABCLeasingABC db = new ABCLeasingABC();

        // GET: Clientes
        public ActionResult Index(int? pageSize, int? page, string searchString)
        {
            pageSize = (pageSize ?? 10);
            page = (page ?? 1);

            ViewBag.PageSize = pageSize;

            ViewData["CurrentFilter"] = searchString;

            var clientes = from c in db.ClientesABC
                select c;

            if (!String.IsNullOrEmpty(searchString))
            {
                clientes = clientes.Where(c => c.nombre.Contains(searchString)
                    || c.RFC.Contains(searchString));
            }
            //return View(db.ClientesABC.ToList());
            return View(clientes.OrderBy(i => i.RFC).ToPagedList(page.Value, pageSize.Value));
        }

        // GET: Clientes/Details/5
        public ActionResult Details(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClientesABC clientesABC = db.ClientesABC.Find(id);
            if (clientesABC == null)
            {
                return HttpNotFound();
            }
            return View(clientesABC);
        }

        // GET: Clientes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "no_cliente,RFC,password,nombre")] ClientesABC clientesABC)
        {
            if (ModelState.IsValid)
            {
                AES aES = new AES();

                string PassDecrypt = clientesABC.password;
                string PassEncrypt = aES.OpenSSLEncrypt(PassDecrypt);
                var UsuarioId = clientesABC.user_id;
                DateTime FechaUpdate = DateTime.Now;

                //#Sección Actualizar Usuario
                var sqlConexionString = ConfigurationManager.ConnectionStrings["ABCLeasing_CS"].ConnectionString;
                SqlConnection sqlConexion = new SqlConnection(sqlConexionString);

                //Guarda al nuevo usuario
                var sqlConsulta = "INSERT INTO ClientesABC (created_at, no_cliente, RFC, password, nombre) VALUES (@created_at, @no_cliente, @RFC, @password, @nombre)";
                sqlConexion.Open();
                SqlCommand sqlComando = new SqlCommand(sqlConsulta, sqlConexion);

                sqlComando.Parameters.AddWithValue("@created_at", FechaUpdate);
                sqlComando.Parameters.AddWithValue("@no_cliente", clientesABC.no_cliente.ToUpper());
                sqlComando.Parameters.AddWithValue("@RFC", clientesABC.RFC.ToUpper());
                sqlComando.Parameters.AddWithValue("@password", PassEncrypt);
                sqlComando.Parameters.AddWithValue("@nombre", clientesABC.nombre.ToUpper());

                sqlComando.ExecuteNonQuery();
                sqlConexion.Close();
                //#end Guardar
                //db.ClientesABC.Add(clientesABC);
                //db.SaveChanges();
                ViewData["CurrentFilter"] = clientesABC.RFC.ToUpper();
                return RedirectToAction("Index");
            }

            return View(clientesABC);
        }

        // GET: Clientes/Edit/5
        public ActionResult Edit(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClientesABC clientesABC = db.ClientesABC.Find(id);
            if (clientesABC == null)
            {
                return HttpNotFound();
            }
            return View(clientesABC);
        }

        // POST: Clientes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "user_id,token,device_type,updated_at,created_at,no_cliente,RFC,password,nombre")] ClientesABC clientesABC)
        {
            if (ModelState.IsValid)
            {
                AES aES = new AES();

                string PassDecrypt = clientesABC.password;
                string PassEncrypt = aES.OpenSSLEncrypt(PassDecrypt);
                var UsuarioId = clientesABC.user_id;
                DateTime FechaUpdate = DateTime.Now;

                //#Sección Actualizar Usuario
                var sqlConexionString = ConfigurationManager.ConnectionStrings["ABCLeasing_CS"].ConnectionString;
                SqlConnection sqlConexion = new SqlConnection(sqlConexionString);

                //Actualiza al Cliente
                string sqlConsulta = "UPDATE ClientesABC SET updated_at = @updated_at, password = @password WHERE user_id = @user_id";
                sqlConexion.Open();
                SqlCommand sqlComando = new SqlCommand(sqlConsulta, sqlConexion);

                sqlComando.Parameters.AddWithValue("@updated_at", FechaUpdate);
                sqlComando.Parameters.AddWithValue("@password", PassEncrypt);
                sqlComando.Parameters.AddWithValue("@user_id", UsuarioId);

                sqlComando.ExecuteNonQuery();
                sqlConexion.Close();
                //#end Actualizar

                //db.Entry(clientesABC).State = EntityState.Modified;
                //db.SaveChanges();
                ViewBag.PageSize = 10;
                ViewBag.Page = 1;
                ViewData["CurrentFilter"] = clientesABC.RFC.ToUpper();
                //return View("Index", clientesABC);
                return RedirectToAction("Index");
            }
            return View(clientesABC);
        }

        // GET: Clientes/Delete/5
        public ActionResult Delete(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClientesABC clientesABC = db.ClientesABC.Find(id);
            if (clientesABC == null)
            {
                return HttpNotFound();
            }
            return View(clientesABC);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(decimal id)
        {
            ClientesABC clientesABC = db.ClientesABC.Find(id);
            db.ClientesABC.Remove(clientesABC);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
