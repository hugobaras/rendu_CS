using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace ConnexionBDD
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            IConfiguration config = builder.Build();

            string connectionString = config.GetConnectionString("WildlensDB");

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:8001")
                .Configure(app =>
                {
                    app.Run(async (context) =>
                    {
                        using (MySqlConnection connection = new MySqlConnection(connectionString))
                        {
                            try
                            {
                                connection.Open();

                                string query = "SELECT * FROM espece";
                                MySqlCommand command = new MySqlCommand(query, connection);
                                MySqlDataReader reader = command.ExecuteReader();

                                List<Dictionary<string, object>> speciesList = new List<Dictionary<string, object>>();

                                while (reader.Read())
                                {
                                    Dictionary<string, object> speciesObject = new Dictionary<string, object>();
                                    speciesObject.Add("espece_id", reader["espece_id"]);
                                    speciesObject.Add("espece_nom", reader["espece_nom"]);
                                    speciesObject.Add("espece_description", reader["espece_description"]);
                                    speciesObject.Add("espece_habitat", reader["espece_habitat"]);
                                    speciesObject.Add("espece_nom_latin", reader["espece_nom_latin"]);
                                    speciesObject.Add("espece_fun_fact", reader["espece_fun_fact"]);
                                    speciesObject.Add("espece_famille", reader["espece_famille"]);
                                    speciesObject.Add("espece_region", reader["espece_region"]);
                                    speciesObject.Add("espece_taille", reader["espece_taille"]);

                                    speciesList.Add(speciesObject);
                                }

                                string json = JsonConvert.SerializeObject(speciesList);
                                context.Response.ContentType = "application/json";
                                await context.Response.WriteAsync(json);

                                reader.Close();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Erreur : {ex.Message}");
                            }
                        }
                    });
                })
                .Build();

            host.Run();
        }
    }
}
