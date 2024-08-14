using DragonBallApiFinal.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DragonBallApiFinal.Controllers
{
    public class CharacterController
    {
        private HttpClient _httpClient = new HttpClient();
        private string baseUrl = "https://dragonball-api.com/api/characters";

        public async Task<Root> GetPersonajes(int page, int limit = 16)
        {
            try
            {
                string url = $"{baseUrl}?limit={limit}&page={page}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Root>(jsonResponse);
                }
                MessageBox.Show("Failed to load characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching characters: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        } 
            
        
    }
}