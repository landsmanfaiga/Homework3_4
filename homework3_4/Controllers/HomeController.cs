using homework3_4.Models;
using homework3_4Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace homework3_4.Controllers
{
    public class HomeController : Controller
    {
        private string _connectionString = "Data Source=.\\sqlexpress;Initial Catalog=SimchaFund;Integrated Security=True;";
        public IActionResult Index()
        {
            SimchaFundManager mgr = new SimchaFundManager(_connectionString);
            SimchosViewModel vm = new SimchosViewModel();
            vm.Simchos = mgr.GetSimchos();
            vm.ContributorCount = mgr.GetContributorCount();
            if (TempData["success-message"] != null)
            {
                vm.Message = (string)TempData["success-message"];
                ViewBag.Message = (string)TempData["success-message"];
            }
            return View(vm);
        }

        [HttpPost]
        public IActionResult New(string name, DateTime date)
        {
            SimchaFundManager mgr = new SimchaFundManager(_connectionString);
            int id = mgr.AddSimcha(name, date);
            TempData["success-message"] = $"New simcha added successfully! id:{id}";
            return Redirect("/home/index");
        }

        public IActionResult Contributors()
        {
            SimchaFundManager mgr = new SimchaFundManager(_connectionString);
            ContributorsViewModel vm = new ContributorsViewModel();
            vm.Contributors = mgr.GetAllContributors();
            vm.Total = mgr.GetCompleteDepositTotal();
            vm.Total -= mgr.GetContribCompleteTotal();
            if (TempData["success-message"] != null)
            {
                vm.Message = (string)TempData["success-message"];
                ViewBag.Message = (string)TempData["success-message"];
            }
            return View(vm);
        }

        [HttpPost]
        public IActionResult Edit(string name, string cell, decimal initialDeposit, DateTime date, bool alwaysInclude)
        {
            SimchaFundManager mgr = new SimchaFundManager(_connectionString);
            int id = mgr.AddContributor(name, cell, alwaysInclude, date);
            mgr.Deposit(id, initialDeposit , date);
            TempData["success-message"]="Contributors Added Successfully!";
            return Redirect("/home/contributors");
        }

        [HttpPost]
        public IActionResult Update(string name, string cell, DateTime date, bool alwaysInclude, int id)
        {
            SimchaFundManager mgr = new SimchaFundManager(_connectionString);
            mgr.UpdateContributor(name, cell,date, alwaysInclude, id);
            TempData["success-message"] = "Contributors Updated Successfully!";
            return Redirect("/home/contributors");
        }

        public IActionResult Contributions(int id)
        {
            SimchaFundManager mgr = new SimchaFundManager(_connectionString);
            ContributionsViewModel vm = new ContributionsViewModel();
            vm.Contributors = mgr.GetContributions(id);
            foreach(Contributor c in vm.Contributors)
            {
                if(c.Contribution > 0)
                {
                    c.Include = true;
                }
                else
                {
                    c.Include = false;
                }
            }
            vm.SimchaName = mgr.GetSimchaName(id);
            vm.Count = 0;
            vm.SimchaId = id;
            return View(vm);
        }

        public IActionResult History(int id)
        {
            SimchaFundManager mgr = new SimchaFundManager(_connectionString);
            HistoryViewModel vm = new HistoryViewModel();
            List<History> history1 = mgr.GetHistory1(id);
            List<History> history2 = mgr.GetHistory2(id);
            history1.AddRange(history2);
            vm.History = history1;
            vm.Name = mgr.GetContributorName(id);
            decimal x = mgr.GetDepositTotal(id);
            x -= mgr.GetContribTotal(id);
            vm.Total = x;
            return View(vm);
        }

        [HttpPost]
        public IActionResult UpdateContributions(List<Contributor> contributors, int simchaId)
        {
            SimchaFundManager mgr = new SimchaFundManager(_connectionString);
            mgr.DeleteContributions(simchaId);
            mgr.AddContributions(contributors, simchaId);
            TempData["success-message"] = "Simcha Updated Successfully!";
            return Redirect("/home/index");
        }

        [HttpPost]
        public IActionResult Deposit(int contributorId, decimal amount, DateTime date)
        {
            SimchaFundManager mgr =new SimchaFundManager(_connectionString);
            mgr.Deposit(contributorId, amount, date);
            TempData["success-message"] = "Deposit Added Successfully!";
            return Redirect("/home/contributors");
        }
         

    }
}