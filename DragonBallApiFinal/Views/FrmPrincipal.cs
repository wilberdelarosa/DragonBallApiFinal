using DragonBallApiFinal.Controllers;
using DragonBallApiFinal.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DragonBallApiFinal.Views
{
    public partial class FrmPrincipal : Form
    {
        private CharacterController controller = new CharacterController();
        private int currentPage = 1;
        private const int limitPerPage = 16;

        // Implementación de caché local
        private Dictionary<int, List<Item>> personajesCache = new Dictionary<int, List<Item>>();


        public FrmPrincipal()
        {
            InitializeComponent();
            DoubleBuffered = true;  // Habilitar DoubleBuffering para suavizar la UI
            InitializeLoadingLabel();  // Inicializar la etiqueta de carga
            LoadAndDisplayCharacters(currentPage);
            SetupNavigationButtons();
        }

        private void FrmPrincipal_Load(object sender, EventArgs e)
        {
        }

        // Inicializar la etiqueta de carga
        private void InitializeLoadingLabel()
        {
            loadingLabel = new Label
            {
                Text = "Cargando datos...",
                AutoSize = false,
                Size = new Size(200, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.Orange,
                BackColor = Color.Transparent,
                Visible = false // Oculto inicialmente
            };

            // Centrar la etiqueta en el panel
            loadingLabel.Location = new Point(
                (flowLayoutPanelPersonajes.Width - loadingLabel.Width) / 2,
                (flowLayoutPanelPersonajes.Height - loadingLabel.Height) / 2
            );

            flowLayoutPanelPersonajes.Controls.Add(loadingLabel);
        }

        // Configura los botones de navegación
        private void SetupNavigationButtons()
        {
            btnPrevious.Click += async (s, e) => await NavigateToPreviousPage();
            btnNext.Click += async (s, e) => await NavigateToNextPage();
        }


        private async void btnNext_ClickAsync(object sender, EventArgs e)
        {
          

        }

        private async void btnPrevious_Click(object sender, EventArgs e)
        {
         
        }

        // Maneja la carga de personajes y la visualización de imágenes con caché
        private async Task LoadAndDisplayCharacters(int page)
        {
            btnPrevious.Enabled = false;
            btnNext.Enabled = false;

            // Mostrar la etiqueta de carga
            ShowLoadingLabel();

            try
            {
                // Verificar si los personajes ya están en caché
                if (personajesCache.ContainsKey(page))
                {
                    // Usar personajes de la caché
                    flowLayoutPanelPersonajes.Controls.Clear();
                    await PopulateCharactersAsync(personajesCache[page]);
                }
                else
                {
                    // Obtener personajes desde la API y almacenarlos en caché
                    var root = await controller.GetPersonajes(page);

                    if (root != null && root.Items != null)
                    {
                        personajesCache[page] = root.Items; // Guardar en caché
                        flowLayoutPanelPersonajes.Controls.Clear();
                        await PopulateCharactersAsync(root.Items);
                        UpdateNavigationButtons(root.Meta);
                    }
                    else
                    {
                        MessageBox.Show("No hay personajes disponibles en esta página.", "Error de carga", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar personajes. Verifique su conexión a internet. Error: {ex.Message}", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Ocultar la etiqueta de carga al terminar
                HideLoadingLabel();
                UpdateButtonState(); // Actualizar estado de los botones
            }
        }

        // Método para mostrar la etiqueta de carga
        private void ShowLoadingLabel()
        {
            loadingLabel.Visible = true;
            loadingLabel.BringToFront(); // Asegurarse de que esté por encima de otros controles
            flowLayoutPanelPersonajes.Controls.Add(loadingLabel); // Añadir si es necesario
            loadingLabel.Location = new Point(
                (flowLayoutPanelPersonajes.Width - loadingLabel.Width) / 2,
                (flowLayoutPanelPersonajes.Height - loadingLabel.Height) / 2
            );
        }

        // Método para ocultar la etiqueta de carga
        private void HideLoadingLabel()
        {
            loadingLabel.Visible = false;
            flowLayoutPanelPersonajes.Controls.Remove(loadingLabel); // Remover de la UI si es necesario
        }

        // Llena el panel con los personajes cargados
        private async Task PopulateCharactersAsync(List<Item> items)
        {
            flowLayoutPanelPersonajes.Controls.Clear();

            foreach (var item in items)
            {
                // Crear y configurar el PictureBox para cada personaje
                PictureBox pictureBox = new PictureBox
                {
                    Size = new Size(180, 180),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Cursor = Cursors.Hand,
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = item // Guardamos el objeto `Item` dentro del `Tag` para futuros usos
                };

                // Cargar la imagen de forma asíncrona
                await LoadAndDisplayImage(item.Image, pictureBox);

                // Etiquetas para el nombre e ID del personaje
                Label nameLabel = new Label
                {
                    Text = item.Name,
                    AutoSize = true,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    ForeColor = Color.White
                };

                Label idLabel = new Label
                {
                    Text = $"ID: {item.Id}",
                    AutoSize = true,
                    Font = new Font("Arial", 9, FontStyle.Regular),
                    ForeColor = Color.LightGray
                };

                // Añadir controles al panel
                flowLayoutPanelPersonajes.Controls.Add(pictureBox);
                flowLayoutPanelPersonajes.Controls.Add(nameLabel);
                flowLayoutPanelPersonajes.Controls.Add(idLabel);
            }
        }

        // Cargar y mostrar imagen en el PictureBox
        private async Task LoadAndDisplayImage(string url, PictureBox pictureBox)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"Error al cargar la imagen: {response.StatusCode}");
                    }

                    using (var imageStream = await response.Content.ReadAsStreamAsync())
                    {
                        var skiaImage = SKBitmap.Decode(imageStream);
                        if (skiaImage == null)
                        {
                            throw new Exception("Error al decodificar la imagen.");
                        }

                        using (var memoryStream = new MemoryStream())
                        {
                            skiaImage.Encode(memoryStream, SKEncodedImageFormat.Png, 100);
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            pictureBox.Image = Image.FromStream(memoryStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la imagen: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Navegar a la página siguiente
        private async Task NavigateToNextPage()
        {
            currentPage++;
            await LoadAndDisplayCharacters(currentPage);
        }

        // Navegar a la página anterior
        private async Task NavigateToPreviousPage()
        {
            if (currentPage > 1)
            {
                currentPage--;
                await LoadAndDisplayCharacters(currentPage);
            }
        }

        // Actualiza el estado de los botones
        private void UpdateButtonState()
        {
            btnPrevious.Enabled = currentPage > 1;
        }

        // Actualiza los botones de navegación con base en los metadatos
        private void UpdateNavigationButtons(Meta meta)
        {
            btnPrevious.Enabled = currentPage > 1;
            btnNext.Enabled = currentPage < meta.TotalPages;
        }
    }
}
