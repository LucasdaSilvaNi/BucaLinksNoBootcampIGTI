using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace BuscaLinksIGTI
{
    class Program
    {

        static string login;
        static string senha;
        static string nomeDoCursoNoPainel;
        static string nomeDaAtividadeParaExtrairLinks;
        static ChromeDriver driver;
        static StreamWriter arquivo;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Digite o Login do IGTI");
                login = Console.ReadLine();

                Console.WriteLine("Digite a senha do IGTI");
                senha = Console.ReadLine();

                Console.WriteLine("Digite o nome do curso que está no painel");
                nomeDoCursoNoPainel = Console.ReadLine();

                Console.WriteLine("Digite o nome do fórum no qual deseja extratir o link");
                nomeDaAtividadeParaExtrairLinks = Console.ReadLine();

                driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                fazerLogin(driver);
                acessarAtividadeComOsLinks(driver);

                if (!Directory.Exists("C://Links do IGTI")) {
                    Directory.CreateDirectory("C://Links do IGTI");
                }

                arquivo = File.CreateText("C://Links do IGTI//Links_" + nomeDaAtividadeParaExtrairLinks.Replace(" ", "_") + ".txt");

                BuscaLinks(driver);

                arquivo.Close();

                Console.WriteLine("");
                Console.WriteLine("E terminou de buscar os links. Verifica se o arquivo está disponvível dentro da pasta C://Links do IGTI na sua máquina");
            }
             catch (Exception e) {

            }
        }

        static void fazerLogin(ChromeDriver driver) {
            driver.Navigate().GoToUrl("https://online.igti.com.br/login/canvas");
            var campoLogin = driver.FindElement(By.CssSelector("#pseudonym_session_unique_id"));
            campoLogin.SendKeys(login);
            var campoSenha = driver.FindElement(By.CssSelector("#pseudonym_session_password"));
            campoSenha.SendKeys(senha);
            var botaoEntrar = driver.FindElement(By.CssSelector(".Button--login"));
            botaoEntrar.Click();
        }

        static void acessarAtividadeComOsLinks(ChromeDriver driver) {
            var curso = driver.FindElement(By.CssSelector("h3[title='"+nomeDoCursoNoPainel+"']"));
            curso.Click();
            var atividade = driver.FindElement(By.CssSelector("a[title='" + nomeDaAtividadeParaExtrairLinks + "']"));
            atividade.Click();
        }

        static void BuscaLinks(ChromeDriver driver) {
            Thread.Sleep(2000);
            var postagens = driver.FindElements(By.CssSelector("div#discussion_subentries ul.discussion-entries li.entry article.discussion_entry div.entry-content div.discussion-section div.user_content"));
            int contadorDeLink = 0;
            string link;
            string linkSemATag;

            foreach (RemoteWebElement post in postagens) {
                contadorDeLink = post.FindElements(By.TagName("a")).Count;
                if (contadorDeLink > 0)
                {
                    link = post.FindElement(By.TagName("a")).GetAttribute("href");
                    arquivo.WriteLine(link);
                }
                else {
                    if (post.Text.Contains("http")) {
                        linkSemATag = post.Text.Split(new string[] { "http" }, StringSplitOptions.None)[1];
                        linkSemATag = linkSemATag.Split("Mês")[0];
                        linkSemATag = linkSemATag.Replace("\r\n", " ");
                        arquivo.WriteLine("http" + linkSemATag.Split(" ")[0]);
                    }
                }
            }

            MudaDePagina(driver);
        }

        static void MudaDePagina(ChromeDriver driver) {
            var paginacao = driver.FindElement(By.CssSelector("div#content div#discussion_container div#discussion_subentries div.discussion-page-nav ul"));
            var contemBotaoPraIrPraUltiaPagina = paginacao.FindElements(By.CssSelector("li a[title='Última']")).Count > 0;

            if (contemBotaoPraIrPraUltiaPagina) {
                string paginaAtualString = paginacao.FindElement(By.CssSelector("li span[title='Atual']")).Text;
                int paginaAtual = Convert.ToInt32(paginaAtualString) + 1;
                var proximaPagina = paginacao.FindElement(By.CssSelector("li a[title='"+ paginaAtual.ToString()+ "']"));
                proximaPagina.Click();
                BuscaLinks(driver);
            }

        }
    }
}
