
///
/// Microexpression Tool
///


using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microexpressions_Training_Tool
{
    public partial class Form1 : Form
    {
        // Pasta onde tem os videos
        string LocalVideos = Application.StartupPath + "\\!Videos\\";

        /// <summary>
        /// Nova lista, ela vai conter todos os vídeos
        /// </summary>
        List<string> videosLocais = new List<string>();
        string videoAtual = null;

        /// <summary>
        /// Função necessário a ser chamada, ela vai iniciar a nossa lista de vídeos
        /// </summary>
        private void ProcurarVideos()
        {
            // Procure todos os .mp4
            foreach (string videosLocal in Directory.GetFiles(LocalVideos, "*.mp4", SearchOption.AllDirectories))
            {
                videosLocais.Add(videosLocal);
            }
        }

        /// <summary>
        /// Altera o cursor
        /// </summary>
        class CursorAlterar
        {
            /// <summary>
            /// Importação da DLL para alterar o cursor
            /// </summary>
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

            // Novo cursor
            private static readonly Cursor CursorMao = new Cursor(LoadCursor(IntPtr.Zero, 32649));

            /// <summary>
            /// Configurar o cursor
            /// </summary>
            public static void AlterarCursor(Control body)
            {
                // Procure todos os controles na FORM
                foreach (Control control in body.Controls)
                {
                    try
                    {
                        // Int
                        int i;

                        // Se for um 
                        if (control.Cursor == Cursors.Hand)
                        {
                            // Altere o cursor
                            control.Cursor = CursorMao;
                        }

                        // Procure outros paineis na FORM
                        for (i = 0; i < 2; i++)
                        {
                            // Sete de novo
                            AlterarCursor(control);
                        }
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            }
        }

        #region TRADUÇÃO

        bool carregando = false;

        private void DetectarLinguaDefinida()
        {
            carregando = true;

            try
            {

                string l = Assembly.GetEntryAssembly().Location;
                string pastaAtual = Path.GetDirectoryName(l);

                string lingua = "";
                if (File.Exists($"{pastaAtual}\\translation\\setting"))
                {
                    lingua = File.ReadAllText($"{pastaAtual}\\translation\\setting");
                }
                else
                {
                    lingua = System.Globalization.CultureInfo.InstalledUICulture.Name;
                }

                if (lingua == "en-US" || lingua == "en")
                {
                    linguagem.SelectedIndex = 1;
                }

                else if (lingua == "pt-BR")
                {
                    linguagem.SelectedIndex = 0;
                }
            }
            catch (Exception)
            {
                linguagem.SelectedIndex = 1;
            }

            carregando = false;

        }

        private void AplicarIdioma()
        {
            // 1️⃣ Descobrir o caminho base
            string l = Assembly.GetEntryAssembly().Location;
            string pastaAtual = Path.GetDirectoryName(l);

            // 2️⃣ Descobrir o idioma definido
            string lingua = "";
            string caminhoSetting = Path.Combine(pastaAtual, "translation", "setting");

            if (File.Exists(caminhoSetting))
            {
                lingua = File.ReadAllText(caminhoSetting).Trim();
            }
            else
            {
                lingua = System.Globalization.CultureInfo.InstalledUICulture.Name;
            }

            // 3️⃣ Ler o arquivo text.ini
            string caminhoTexto = Path.Combine(pastaAtual, "translation", "text.ini");
            if (!File.Exists(caminhoTexto))
            {
                MessageBox.Show("Arquivo de tradução não encontrado!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Lê todo o texto do arquivo
            string conteudo = File.ReadAllText(caminhoTexto);

            // Divide em blocos por idioma
            string blocoEN = "";
            string blocoPT = "";

            // Localiza os marcadores "en:::" e "pt-BR:::" e pega os textos entre eles
            int idxEn = conteudo.IndexOf("en:::");
            int idxPt = conteudo.IndexOf("pt-BR:::");

            if (idxEn >= 0 && idxPt > idxEn)
            {
                blocoEN = conteudo.Substring(idxEn + "en:::".Length, idxPt - (idxEn + "en:::".Length)).Trim();
            }

            if (idxPt >= 0)
            {
                blocoPT = conteudo.Substring(idxPt + "pt-BR:::".Length).Trim();
            }

            // Seleciona o bloco correto
            string blocoSelecionado = lingua.StartsWith("pt", StringComparison.OrdinalIgnoreCase) ? blocoPT : blocoEN;



            // 5️⃣ Ler as linhas do bloco e montar o dicionário
            var traducoes = new Dictionary<string, string>();

            foreach (string linha in blocoSelecionado.Split('\n'))
            {
                string lnh = linha.Trim();
                if (string.IsNullOrWhiteSpace(lnh)) continue;
                if (!lnh.StartsWith("[")) continue;

                // Exemplo: [label1]="Selecione a emoção:"
                int chaveInicio = lnh.IndexOf('[') + 1;
                int chaveFim = lnh.IndexOf(']');
                if (chaveInicio < 0 || chaveFim < 0) continue;

                string chave = lnh.Substring(chaveInicio, chaveFim - chaveInicio);
                int valorInicio = lnh.IndexOf('"') + 1;
                int valorFim = lnh.LastIndexOf('"');
                if (valorInicio < 0 || valorFim < 0) continue;

                string valor = lnh.Substring(valorInicio, valorFim - valorInicio);
                traducoes[chave] = valor;
            }

            // 6️⃣ Aplicar as traduções nos controles
            foreach (Control ctrl in this.Controls)
            {
                if (traducoes.ContainsKey(ctrl.Name))
                {
                    ctrl.Text = traducoes[ctrl.Name];
                }
            }

            // (Opcional) - também traduz subcontroles (ex: dentro de painéis ou groupboxes)
            TraduzirSubControles(this.Controls, traducoes);
        }

        private void TraduzirSubControles(Control.ControlCollection controls, Dictionary<string, string> traducoes)
        {
            foreach (Control c in controls)
            {
                if (traducoes.ContainsKey(c.Name))
                    c.Text = traducoes[c.Name];

                if (c.HasChildren)
                    TraduzirSubControles(c.Controls, traducoes);
            }
        }
        

        #endregion

        /// <summary>
        /// Depois de iniciar o programa
        /// </summary>
        public Form1()
        {
            ProcurarVideos();
            ProximoVideo();
            InitializeComponent();

            CursorAlterar.AlterarCursor(this);

            axWindowsMediaPlayer1.Enabled = false;
            axWindowsMediaPlayer1.settings.volume = 0;
            axWindowsMediaPlayer1.URL = videoAtual;

            DetectarLinguaDefinida();
            AplicarIdioma();
        }

        /// <summary>
        /// Reseta os botões
        /// </summary>
        private void ResetarBotoes()
        {
            foreach (Guna2GradientButton botoes in Controls.OfType<Guna2GradientButton>())
            {
                botoes.ForeColor = Color.Black;
                botoes.FillColor = Color.FromArgb(231, 231, 231);
                botoes.FillColor2 = Color.FromArgb(231, 231, 231);
            }
        }

        /// <summary>
        /// Verifica se o usuário acertou
        /// </summary>
        /// <param name="botao"></param>
        /// <returns></returns>
        private bool Acertou(Guna2GradientButton botao)
        {
            botao.ForeColor = Color.White;
            string emocao = Path.GetDirectoryName(videoAtual).Replace(LocalVideos, "");

            // Verique se acertou a emoção
            if (botao.Name.ToLower() == emocao.ToLower())
            {
                proximo.Visible = true;
                botao.FillColor = Color.MediumSpringGreen;
                botao.FillColor2 = Color.MediumSpringGreen;

                return true;
            }

            botao.FillColor = Color.Tomato;
            botao.FillColor2 = Color.Tomato;


            return false;
        }

        /// <summary>
        /// Botão de emoções
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Botao_Click(object sender, EventArgs e)
        {
            Acertou((sender as Guna2GradientButton));
        }

        /// <summary>
        /// Acha o próximo vídeoe
        /// </summary>
        private void ProximoVideo()
        {
            videoAtual = videosLocais.OrderBy(s => Guid.NewGuid()).First();
        }

        /// <summary>
        /// Proximo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void proximo_Click(object sender, EventArgs e)
        {
            ResetarBotoes();
            proximo.Visible = false;
            ProximoVideo();

            // Configure o próximo vídeo
            axWindowsMediaPlayer1.URL = videoAtual;
            axWindowsMediaPlayer1.Ctlcontrols.play();

        }

        private void linguagem_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (carregando == true)
                    return;

                int selecionado = linguagem.SelectedIndex;
                string lingua = "";

                // EUA
                if (selecionado == 1)
                {
                    // Vazio, por padrão, é inglês
                    lingua = "en-US";
                }

                // PORTUGUÊS BRAZIL
                else if (selecionado == 0)
                {
                    lingua = "pt-BR";
                }

                string l = Assembly.GetEntryAssembly().Location;
                string pastaAtual = Path.GetDirectoryName(l);

                File.WriteAllText($"{pastaAtual}\\translation\\setting", lingua);

                System.Diagnostics.Process.Start(l);
                Environment.Exit(0);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void paypal_Click(object sender, EventArgs e)
        {
            MessageBox.Show("dimasperreira595paypal@gmail.com");
        }
    }
}
