using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Xml.Linq;
using System.Xml.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace homework3_4Data
{
    public class Simcha
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ContributorCount { get; set; }
        public decimal Total {  get; set; }
        public DateTime Date { get; set; }

    }

    public class Contributor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Cell { get; set; }
        public bool AlwaysInclude { get; set; }
        public decimal Balance { get; set; }
        public decimal Contribution { get; set; }

        public decimal InitialDeposit { get; set; }

        public DateTime DateCreated { get; set; }

        public bool Include {  get; set; }

    }

    public class History
    {
        public string Action { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }

    public class SimchaFundManager
    {
        private string _connectionString;
        public SimchaFundManager(string connectionString)
        {
          _connectionString = connectionString;    
        }

        public List<Simcha> GetSimchos()
        {
            using var conn = new SqlConnection ( _connectionString );
            using var cmd = conn.CreateCommand ();
            cmd.CommandText = $@"SELECT * FROM Simchos";
            conn.Open();
            List<Simcha> simchos = new List<Simcha>();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Simcha s = new Simcha();
                s.Id = (int)reader["Id"];
                s.Name = (string)reader["Name"];
                s.Date = (DateTime)reader["Date"];
                s.Total = GetContribTotalForSimcha(s.Id);
                s.ContributorCount = GetContribAmountForSimcha(s.Id);
               simchos.Add(s);
            }
            return simchos;
        }

        public int AddSimcha(string name, DateTime date)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"INSERT INTO Simchos(Name, Date)
                                  VALUES(@name, @date)SELECT SCOPE_IDENTITY()";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@date", date);
            conn.Open();
            return (int)(decimal)cmd.ExecuteScalar();
        }

        public int GetContributorCount()
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"SELECT COUNT(*) FROM Contributors";
            conn.Open();
            return (int)cmd.ExecuteScalar();

        }

        public List<Contributor> GetAllContributors()
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"SELECT * FROM Contributors";
            conn.Open();
            List<Contributor> contributors = new List<Contributor>();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Contributor c = new Contributor();
                c.Id = (int)reader["Id"];
                c.Name = (string)reader["Name"];
                c.Cell = (string)reader["Cell"];
                c.AlwaysInclude = (bool)reader["AlwaysInclude"];
                c.DateCreated = (DateTime)reader["DateCreated"];
                c.Balance = GetDepositTotal(c.Id);
                c.Balance -= GetContribTotal(c.Id);
                contributors.Add(c);
            }
            return contributors;
        }

        public int AddContributor(string name, string cell, bool alwaysInclude, DateTime dateCreated)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"INSERT INTO Contributors(Name, Cell, AlwaysInclude, DateCreated)
                                  VALUES(@name, @cell, @alwaysInclude, @dateCreated)SELECT SCOPE_IDENTITY()";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@cell", cell);
            cmd.Parameters.AddWithValue("@alwaysInclude", alwaysInclude);
            cmd.Parameters.AddWithValue("@dateCreated", dateCreated);
            conn.Open();
            return (int)(decimal)cmd.ExecuteScalar();
        }

        public List<Contributor> GetContributions(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"SELECT c.id, c.Name, c.AlwaysInclude
                                    FROM Contributors c";
            conn.Open();
            List<Contributor> contributions = new List<Contributor>();
            SqlDataReader reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                Contributor c = new Contributor();
                c.Id = (int)reader["Id"];
                c.Name = (string)reader["Name"];
                c.AlwaysInclude = (bool)reader["AlwaysInclude"];
                c.Balance = GetDepositTotal(c.Id);
                c.Balance -= GetContribTotal(c.Id);
                c.Contribution = GetAmount(id, c.Id);
                contributions.Add(c);
      
            }
            return contributions;
           
        }
        public decimal GetAmount(int simchaId, int contributorId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"Select Contribution from Contributions 
                                    where SimchaId = @simchaId and ContributorId = @contributorId";
            cmd.Parameters.AddWithValue("@simchaId", simchaId);
            cmd.Parameters.AddWithValue("@contributorId", contributorId);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                decimal amount = (decimal)reader["Contribution"];
                return amount;
            }
            return 0;
        }

        public string GetSimchaName(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Name FROM Simchos WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id );
            conn.Open();
            return (string)cmd.ExecuteScalar();
        }

        public List<History> GetHistory1(int id)
        {
            using var conn = new SqlConnection(_connectionString) ;
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"Select c.Contribution, c.Date, s.Name from Contributions c
                                    join Simchos s
                                    on s.Id = c.SimchaId
                                    where c.ContributorId = @id";
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            List<History> history = new List<History>();
            while(reader.Read())
            {
                history.Add(new()
                {
                    Action = (string)reader["Name"],
                    Date = (DateTime)reader["Date"],
                    Amount = (decimal)reader["Contribution"]
                });

            }

            return history;
            
        }

        public List<History>GetHistory2(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"select d.Deposit, d.Date from Deposits d
                                    where d.ContributorId = @id";
            cmd.Parameters.AddWithValue("@id", id);
            List<History> history = new List<History>();
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                History h = new History();
                h.Date = (DateTime)reader["Date"];
                h.Amount = (decimal)reader["Deposit"];
                h.Action = "Deposit";
                history.Add(h);
            }

            return history;

        }

        public string GetContributorName(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Name FROM Contributors WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            return (string)cmd.ExecuteScalar();
        }

        public decimal GetContribTotal(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"select c.Contribution from Contributions c
                                    where c.ContributorId = @id";
            cmd.Parameters.AddWithValue("@id", id);
            decimal x = 0;
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                x += (decimal)reader["Contribution"];
            }
            return x;
        }
        public decimal GetDepositTotal(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"select d.Deposit from Deposits d
                                    where d.ContributorId = @id";
            cmd.Parameters.AddWithValue("@id", id);
            decimal x = 0;
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                x += (decimal)reader["Deposit"];
            }
            return x;
        }

        public decimal GetContribTotalForSimcha(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"select Contribution from Contributions
                                where SimchaId = @id";
            cmd.Parameters.AddWithValue("@id", id);
            decimal x = 0;
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                x += (decimal)reader["Contribution"];
            }
            return x;
        }

        public int GetContribAmountForSimcha(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"select Contribution from Contributions
                                where SimchaId = @id";
            cmd.Parameters.AddWithValue("@id", id);
            int x = 0;
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                x ++;
            }
            return x;
        }

        public void AddContribution(Contributor c, int simchaId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"insert into Contributions(SimchaId, ContributorId, Contribution, Date)
                                values(@simchaid, @contribid, @contribution, @date)";
            cmd.Parameters.AddWithValue("@simchaid", simchaId);
            cmd.Parameters.AddWithValue("@contribid", c.Id);
            cmd.Parameters.AddWithValue("@contribution", c.Contribution);
            cmd.Parameters.AddWithValue("@date", DateTime.Now);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void AddContributions(List<Contributor> contributors, int simchaId)
        {
            foreach(var contributor in contributors)
            {
                if (contributor.Include == true)
                {
                    AddContribution(contributor, simchaId);
                }


            }
        }

        public void DeleteContributions(int simchaId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"delete from Contributions
                                where SimchaId = @simchaId";
            cmd.Parameters.AddWithValue("@simchaId", simchaId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void Deposit(int contributorId, decimal amount, DateTime date)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"insert into Deposits(ContributorId, Deposit, Date)
                                    values(@contributorid, @amount ,@date)";
            cmd.Parameters.AddWithValue("@contributorid", contributorId);
            cmd.Parameters.AddWithValue("@amount", amount);
            cmd.Parameters.AddWithValue("@date", date);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public decimal GetContribCompleteTotal()
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"select c.Contribution from Contributions c";
            decimal x = 0;
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                x += (decimal)reader["Contribution"];
            }
            return x;
        }
        public decimal GetCompleteDepositTotal()
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"select d.Deposit from Deposits d";
            decimal x = 0;
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                x += (decimal)reader["Deposit"];
            }
            return x;
        }

        public void UpdateContributor(string name, string cell, DateTime date, bool alwaysInclude, int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"update Contributors
                                set Name = @name, Cell = @cell, AlwaysInclude = @alwaysInclude, DateCreated = @date
                                where Id = @id";
            cmd.Parameters.AddWithValue("@name",name);
            cmd.Parameters.AddWithValue("@cell", cell);
            cmd.Parameters.AddWithValue("@alwaysInclude", alwaysInclude);
            cmd.Parameters.AddWithValue("@date", date);
            cmd.Parameters.AddWithValue("@id",id);
            conn.Open();
            cmd.ExecuteNonQuery();
        }


    }
    
    public class SimchosViewModel
    {
        public List<Simcha> Simchos { get; set; }
        public int ContributorCount { get; set; }

        public string Message { get; set; }
    }

    public class ContributorsViewModel
    {
        public List<Contributor> Contributors { get; set; }

        public string Message { get; set; }

        public decimal Total { get; set; }
    }

    public class ContributionsViewModel
    {
        public List <Contributor> Contributors { get;set; }
        public int SimchaId { get; set; }
        public string SimchaName { get; set; }
        public int Count { get; set; }
    }

    public class HistoryViewModel
    {
        public List<History> History { get; set; }
        public decimal Total { get; set; }
        public string Name { get; set; }
    }

}