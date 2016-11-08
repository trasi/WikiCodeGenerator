using System;
using RB.Utils.WikiCodeGenerator.ConfluenceService;
using System.Configuration;
using System.Text.RegularExpressions;

namespace RB.Utils.WikiCodeGenerator
{
    internal class ConfluenceProxy : IDisposable
    {
        ConfluenceSoapServiceService confluence = new ConfluenceSoapServiceService();

        string wiki_access_token;

        string space;
        long grandparent_page_id;

        const string DEFAULT_SPACE = "B2BD";
        const int DEFAULT_GRANDPARENT_PAGE_ID = 48893630;

        const string VERSION_COMMENT = "Uppfærð útgáfa smíðuð af WikiCodeGenerator.";
        const string EMPTY_PARENT_PAGE = @"<h1>Inngangur</h1>
                                            <ac:macro ac:name='excerpt'>
                                            <ac:parameter ac:name='atlassian-macro-output-type'>BLOCK</ac:parameter>
                                              <ac:rich-text-body>
                                                <p>{0} er þjónusta ...</p>
                                              </ac:rich-text-body>
                                            </ac:macro>
                                            <h1>Vefslóðir</h1>
                                            <p>
                                              <strong>Raunumhverfi: ...</strong>
                                            </p>
                                            <p>
                                              <strong>Prófunarumhverfi: ...</strong>
                                            </p>
                                            <p>
                                              <strong>Þróunarumhverfi: ...</strong>
                                            </p>
                                            <h2>Aðgerðir</h2>
                                            <p>
                                              <ac:macro ac:name='children'>
                                                <ac:parameter ac:name='sort'>title</ac:parameter>
                                                <ac:parameter ac:name='style'>h4</ac:parameter>
                                                <ac:parameter ac:name='excerpt'>true</ac:parameter>
                                              </ac:macro>
                                            </p>";

        /// <summary>
        /// Smiður sem tekur við Wiki-notandanafni og -lykilorði til þess að auðkenna notanda gagnvart RB Wiki.
        /// </summary>
        /// <param name="user">Wiki-notendanafn. Sé gildi þessa viðfangs null eða tómi strengurinn er Wiki-notendanafn úr stillingaskrá (App.config) notað.</param>
        /// <param name="password">Wiki-lykilorð. Sé gildi þessa viðfangs null eða tómi strengurinn er Wiki-lykilorð úr stillingaskrá (App.config) notað.</param>
        public ConfluenceProxy(string user, string password)
        {
            space = String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["confluence.space"])? 
                DEFAULT_SPACE: 
                ConfigurationManager.AppSettings["confluence.space"];
            grandparent_page_id = String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["confluence.grandparentpage"]) ?
                DEFAULT_GRANDPARENT_PAGE_ID :
                Convert.ToInt64(ConfigurationManager.AppSettings["confluence.grandparentpage"]);

            user = string.IsNullOrWhiteSpace(user) ? ConfigurationManager.AppSettings["confluence.user"] : user;
            password = string.IsNullOrWhiteSpace(password) ? ConfigurationManager.AppSettings["confluence.password"] : password;

            wiki_access_token = confluence.login(user, password);
        }

        /// <summary>
        /// Smiður sem notar Wiki-notandanafn og -lykilorð úr stillingaskrá (App.config) til þess að auðkenna notanda gagnvart RB Wiki.
        /// </summary>
        public ConfluenceProxy()
            : this(null, null)
        {
        }

        /// <summary>
        /// Uppfærir Wiki-skjal Torg-þjónustuaðgerðar með viðkomandi Wiki-kóða.
        /// </summary>
        /// <param name="service">Nafn Torg-þjónustu.</param>
        /// <param name="operation">Nafn Torg-þjónustuaðgerðar.</param>
        /// <param name="description">Wiki-kóði sem með skal uppfæra Wiki-skjal viðkomandi Torg-þjónustuaðgerðar.</param>
        /// <param name="overwrite">Ef ekki true er reynt að uppfæra aðeins Lýsingar- Inntaks-, Úttaks- og Dæmskafla Wiki-skjals en halda öðrum 
        /// hlutum þess óbreyttum, annars er Wiki-skjal yfirskrifað.</param>
        /// <returns>URL uppfærðs Wiki-skjals.</returns>
        public string UpdateWiki(string service, string operation, string description, bool overwrite)
        {
            Tuple<long, long> page_ids = GetPageIDs(String.Format("{0}.{1}", service, operation), service);
            
            if (page_ids.Item1 == -1)
                page_ids = GetPageIDs(operation, service);

            long parent_page_id = page_ids.Item2;

            if (parent_page_id == -1)
            {
                if (String.IsNullOrEmpty(service))
                    throw new Exception("Unable to create parent page. Please add your service name to WSDL document. For further details, refer to WikiCodeGenerator's documnetation at https://wiki.rb.is.");
                
                RemotePage parent_page = CreatePage(service, String.Format(EMPTY_PARENT_PAGE, service), grandparent_page_id);
                parent_page_id = parent_page.id;
            }

            RemotePage page = (page_ids.Item1 == -1) ?
                CreatePage(String.Format("{0}.{1}", service, operation), description, parent_page_id) :
                UpdatePage(page_ids.Item1, String.Format("{0}.{1}", service, operation), description, overwrite);

            return page.url;
        }

        /// <summary>
        /// Uppfærir tilgreint Wiki-skjal með viðkomandi Wiki-kóða.
        /// </summary>
        /// <param name="url">URL Wiki-skjals.</param>
        /// <param name="title">Titill Wiki-skjals.</param>
        /// <param name="description">Wiki-kóði sem með skal uppfæra viðkomandi Wiki-skjal.</param>
        /// <param name="overwrite">Ef ekki true er reynt að uppfæra aðeins Lýsingar- Inntaks-, Úttaks- og Dæmskafla Wiki-skjals en halda öðrum 
        /// hlutum þess óbreyttum, annars er Wiki-skjal yfirskrifað.</param>
        /// <returns>URL uppfærðs Wiki-skjals.</returns>
        public string UpdateWikiURL(string url, string title, string description, bool overwrite)
        {
            long page_id = GetPageID(url);

            if (page_id == -1)
                throw new Exception(String.Format("Unable to locate Wiki-page in space {0} at {1}.", space, url));

            RemotePage page = UpdatePage(page_id, title, description, overwrite);

            return page.url;
        }

        /// <summary>
        /// Uppfærir Wiki-skjal eftir númeri.
        /// </summary>
        /// <param name="page_id">Númer Wiki-skjals sem skal uppfæra.</param>
        /// <param name="content">Wiki-kóði sem með skal uppfæra viðkomandi Wiki-skjal.</param>
        /// <param name="overwrite">Ef ekki true er reynt að uppfæra aðeins Lýsingar- Inntaks-, Úttaks- og Dæmskafla Wiki-skjals en halda öðrum 
        /// hlutum þess óbreyttum, annars er Wiki-skjal yfirskrifað.</param>
        /// <returns>RemotePage-hlutur sem samsvarar uppfærðu Wiki-skjali.</returns>
        private RemotePage UpdatePage(long page_id, string page_title, string content, bool overwrite)
        {
            RemotePage page = confluence.getPage(wiki_access_token, page_id);
            
            page.content = overwrite ? content : Helper.MergePage(content, page.content);
            page.title = page_title;
            
            return confluence.updatePage(wiki_access_token, page, new RemotePageUpdateOptions() { versionComment = VERSION_COMMENT });
        }       

        /// <summary>
        /// Býr til nýtt Wiki-skjal.
        /// </summary>
        /// <param name="title">Titil Wiki-skjals.</param>
        /// <param name="content">Wiki-kóði viðkomandi Wiki-skjals.</param>
        /// <param name="parent_id">Númer Wiki-skjals sem nýtt Wiki-skal sett undir.</param>
        /// <returns>RemotePage-hlutur sem samsvarar nýja Wiki-skjalinu.</returns>
        private RemotePage CreatePage(string title, string content, long parent_id)
        {
            RemotePage page = new RemotePage() { space = space, title = title, content = content, parentId = parent_id };

            return confluence.storePage(wiki_access_token, page);
        }

        /// <summary>
        /// Finnur númer Wiki-skjala eftir nöfnum þeirra, þar sem fyrra skjalið er barn þess síðara.
        /// </summary>
        /// <param name="parent_title">Nafn fyrra Wiki-skjals (barns).</param>
        /// <param name="page_title">Nafn seinna Wiki-skjals (foreldra).</param>
        /// <returns>Tuple<long,long>-hlutur þar sem fyrra stakið er númer fyrra skjalsins og seinna stakið númer seinna skjalsins. 
        /// Ef annað hvort stakið er -1 merkir það að samsvarandi Wiki-skjal hafi ekki fundist. Hafi fyrra Wiki-skjalið fundist en foreldri þess
        /// samkvæmt Wiki er ekki sá sem fannst við leit eftir nafni er kastað viðeigandi villu.</returns>
        private Tuple<long, long> GetPageIDs(string page_title, string parent_title)
        {
            long page_id = -1;
            long parent_id = -1;

            long alleged_parent_id = -1;

            RemotePageSummary[] pages = confluence.getPages(wiki_access_token, space);

            foreach (RemotePageSummary page in pages)
            {
                if (page.title == page_title)
                {
                    page_id = page.id;
                    alleged_parent_id = page.parentId;
                }
                else if (page.title == parent_title)
                {
                    parent_id = page.id;
                }
            }

            // ef barn og foreldri finnast samkvæmt tilgreindum nöfnum en barn telur sig tilheyra öðru foreldri ...
            if (!(page_id == -1 || parent_id == -1 || parent_id == alleged_parent_id))
                throw new Exception(String.Format("Child-parent mismatch: Found parent is #{0}, alleged parent is #{1}.", parent_id.ToString(), alleged_parent_id.ToString()));
           
            // ef foreldri finnst ekki er barni treyst ...
            if (parent_id == -1 && alleged_parent_id != -1)
                parent_id = alleged_parent_id;

            return new Tuple<long, long>(page_id, parent_id);
        }

        /// <summary>
        /// Finnur númer Wiki-skajs eftir URL þess.
        /// </summary>
        /// <param name="url">Meint URL Wiki-skjals.</param>
        /// <returns>Númer Wiki-skals. Ef ekkert skjal finnst í viðkomandi space í Wiki með umrætt URL er skilagildið -1.</returns>
        private long GetPageID(string url)
        {
            long page_id = -1;

            RemotePageSummary[] pages = confluence.getPages(wiki_access_token, space);

            foreach (RemotePageSummary page in pages)
            {
                if (page.url == url)
                {
                    page_id = page.id;
                    break;
                }
            }

            return page_id;
        }

        /// <summary>
        /// Dispose-aðgerð.
        /// </summary>
        public void Dispose()
        {
            confluence.logout(wiki_access_token);
            confluence.Dispose();
        }
    }
}
