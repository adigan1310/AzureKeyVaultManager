using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AzureKeyManager.Models;
using System.IO;
using System.Web;
using Newtonsoft.Json;

namespace AzureKeyManager.Controllers
{
    public class HomeController : Controller
    {
        string path = System.IO.Directory.GetCurrentDirectory() + "\\config.txt";
        public static List<core.DataList> globlist = new List<core.DataList>();
        AzureKeyManager.core.AzureKeyManagerInterface netManagerInterface = new AzureKeyManager.Data.AzureKeyManagerData();
        public IActionResult Index()
        {
            ViewData["Success"] = TempData["Success"];
            return View();
        }

        public IActionResult Config()
        {
            core.AzureKeyManager azureNet = new core.AzureKeyManager();
            try
            {
                var fileContents = System.IO.File.ReadAllLines(path);
                azureNet.ClientID = fileContents[0];
                azureNet.SecretKey = fileContents[1];
                azureNet.TenantID = fileContents[2];
                azureNet.URi = fileContents[3];
            }
            catch(Exception)
            {

            }
            return View(azureNet);
        }

        public IActionResult Export()
        {
            core.AzureKeyManager azureNet = new core.AzureKeyManager();
            try
            {
                var fileContents = System.IO.File.ReadAllLines(path);
                if (fileContents != null)
                {
                    azureNet.ClientID = fileContents[0];
                    azureNet.SecretKey = fileContents[1];
                    azureNet.TenantID = fileContents[2];
                    azureNet.URi = fileContents[3];
                }
                List<core.DataList> datalist = netManagerInterface.getKeyList(azureNet);
                return View(datalist);
            }
            catch(FileNotFoundException)
            {
                TempData["Success"] = "Please provide configuration details...";
                return RedirectToAction("Index");
            }
            
        }

        public IActionResult Import()
        {
            ViewData["Message"] = TempData["Error"];
            return View();
        }

        [HttpPost]
        public IActionResult Importfile(core.AzureKeyManager azureNet)
        {
            if (azureNet.file == null || azureNet.URi == null)
            {
                TempData["Error"] = "Both fields are required!!!";
                return RedirectToAction("Import");
            }
            try
            {   
            List<core.DataList> datalist = new List<core.DataList>();
                using (StreamReader sr = new StreamReader(azureNet.file))
            {              
                datalist = JsonConvert.DeserializeObject<List<core.DataList>>(sr.ReadToEnd());
            }
            datalist.Sort((x, y) => x.secretvalue.CompareTo(y.secretvalue));
            List<core.DataList> list = datalist.GroupBy(x => x.secretname).Select(y => y.First()).ToList();
            TempData["URi"] = azureNet.URi;
            ViewData["duplicatecount"] = datalist.Count() - list.Count();
            return View(list);
            }
            catch(Exception e)
            {
                TempData["Error"] = "Secret Values has unknown character.\\nError: " + e.ToString();
                return RedirectToAction("Import");
            }
        }

        [HttpGet]
        public IActionResult Importfile(string serializedModel)
        {
            ViewData["Error"] = TempData["PublishError"];
            List<core.DataList> datalist = globlist;
            datalist.Sort((x, y) => x.secretvalue.CompareTo(y.secretvalue));
            ViewData["duplicatecount"] = 0;
            return View(datalist);
        }

        [HttpPost]
        public IActionResult Loadfile(List<core.DataList> datalist)
        {
            core.AzureKeyManager azureNet = new core.AzureKeyManager();
            try
            {
                var fileContents = System.IO.File.ReadAllLines(path);
                bool result = false;
                int rowcount = 0;
                if(fileContents != null)
                {   
                    azureNet.ClientID = fileContents[0];
                    azureNet.SecretKey = fileContents[1];
                    azureNet.TenantID = fileContents[2];
                    azureNet.URi = TempData["URi"].ToString();
                }
                var templist = datalist.Where(m => m.isChecked == true).ToList();
                if (templist.Any())
                {
                    result = netManagerInterface.submitKey(azureNet, templist);
                    rowcount = templist.Count();
                }
                else
                {
                    result = netManagerInterface.submitKey(azureNet, datalist);
                    rowcount = datalist.Count();
                }                      
                if(result)
                {
                    TempData["PublishError"] = rowcount.ToString() + " Secrets stored successfully.";
                    datalist.Where(m => m.isChecked == true).ToList().ForEach(s => s.isChecked = false);
                    datalist.Where(m => m.secretvalue == null).ToList().ForEach(s => s.secretvalue = "");
                    globlist = datalist;
                }
                else
                {
                    TempData["PublishError"] = "Issue in storing secrets."; 
                    globlist = datalist;           
                }
            }
            catch(Exception)
            {
                TempData["PublishError"] = "Configuration Settings not found.";
                globlist = datalist;
            }
            return RedirectToAction("Importfile", new { serializedModel = JsonConvert.SerializeObject("reloading") });
        }

        [HttpPost]
        public IActionResult Config(AzureKeyManager.core.AzureKeyManager azureNet)
        {
            bool result = netManagerInterface.ConnectionVerification(azureNet);
            if (result)
            {
                TempData["Success"] = "Configuration saved...";
                string[] lines = { azureNet.ClientID, azureNet.SecretKey, azureNet.TenantID, azureNet.URi };
                System.IO.File.WriteAllLines(path, lines);
                return RedirectToAction("Index");
            }
            ViewData["Error"] = "Unable to connect to Azure Key Vault...";
            return View();
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
