using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace bancomat2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var context = new bancomatEntities())
            {
                Console.WriteLine("Benvenuto al programma Bankomat!");

                // Effettua la selezione della banca e l'autenticazione
                Utenti utenteAutenticato = SelezionaBancaEAutentica(context);

                if (utenteAutenticato != null)
                {
                    Console.WriteLine($"Benvenuto, {utenteAutenticato.NomeUtente}!");

                    // Entra nel menu principale
                    MenuPrincipale(context, utenteAutenticato);
                }
                else
                {
                    Console.WriteLine("Accesso negato. Utente bloccato o dati di accesso errati.");
                }
            }

            Console.WriteLine("Grazie per aver utilizzato Bankomat. Arrivederci!");
        }

        static Utenti SelezionaBancaEAutentica(bancomatEntities context)
        {
            int tentativiRimasti = 3;

            while (tentativiRimasti > 0)
            {
                Console.WriteLine("\nSeleziona una banca:");
                MostraBancheDisponibili(context);

                Console.Write("Nome Banca: ");
                string nomeBanca = Console.ReadLine();

                Banche bancaSelezionata = TrovaBancaPerNome(context, nomeBanca);

                if (bancaSelezionata != null)
                {
                    Console.Write("Nome Utente: ");
                    string nomeUtente = Console.ReadLine();

                    Console.Write("Password: ");
                    string password = Console.ReadLine();

                    Utenti utenteAutenticato = AutenticaUtente(context, bancaSelezionata, nomeUtente, password);

                    if (utenteAutenticato != null)
                    {
                        return utenteAutenticato; // Restituisce l'utente autenticato
                    }
                    else
                    {
                        tentativiRimasti--;
                        Console.WriteLine($"Accesso negato. Tentativi rimasti: {tentativiRimasti}");
                    }
                }
                else
                {
                    Console.WriteLine("Banca non valida. Riprova.");
                }
            }

            Console.WriteLine("Utente bloccato. Contatta l'assistenza.");
            return null;
        }

        static void MostraBancheDisponibili(bancomatEntities context)
        {
            var banche = context.Banches.ToList();

            foreach (var banca in banche)
            {
                Console.WriteLine($"- {banca.Nome}");
            }
        }

        static Banche TrovaBancaPerNome(bancomatEntities context, string nomeBanca)
        {
            return context.Banches.FirstOrDefault(b => b.Nome.Equals(nomeBanca, StringComparison.OrdinalIgnoreCase));
        }

        static Utenti AutenticaUtente(bancomatEntities context, Banche banca, string nomeUtente, string password)
        {
            return context.Utentis
                    .FirstOrDefault(u => u.IdBanca == banca.Id &&
                    u.NomeUtente.Equals(nomeUtente, StringComparison.OrdinalIgnoreCase) &&
                    u.Password == password &&
                    !u.Bloccato);
        }

        static string SelezionaBanca()
        {
            Console.WriteLine("Benvenuto a Bankomat!");
            Console.WriteLine("Seleziona una banca:");

            using (var context = new bancomatEntities())
            {
                var banche = context.Banches.Select(b => b.Nome).ToList();

                for (int i = 0; i < banche.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {banche[i]}");
                }

                int scelta;
                if (int.TryParse(Console.ReadLine(), out scelta) && scelta >= 1 && scelta <= banche.Count)
                {
                    return banche[scelta - 1];
                }
            }

            return null;
        }

        static bool AutenticazioneUtente(bancomatEntities context, string nomeBanca)
        {
            Console.WriteLine("Inserisci il tuo nome utente:");
            string nomeUtente = Console.ReadLine();
            Console.WriteLine("Inserisci la tua password:");
            string password = Console.ReadLine();

            // Cerca l'utente nel database in base al nomeUtente e alla banca selezionata
            var utente = context.Utentis.FirstOrDefault(u => u.NomeUtente == nomeUtente && u.Banche.Nome == nomeBanca);

            if (utente != null)
            {
                if (!utente.Bloccato)
                {
                    if (utente.Password == password)
                    {
                        Console.WriteLine("Accesso riuscito. Benvenuto!");
                        return true; // Utente autenticato con successo
                    }
                    else
                    {
                        Console.WriteLine("Password errata. Riprova.");
                        // Aggiorna il conteggio dei tentativi falliti per l'utente
                        utente.TentativiFalliti++;
                        if (utente.TentativiFalliti >= 3)
                        {
                            Console.WriteLine("Hai superato il limite di tentativi falliti. L'utente verrà bloccato.");
                            utente.Bloccato = true;
                        }
                        context.SaveChanges(); // Salva le modifiche al database
                    }
                }
                else
                {
                    Console.WriteLine("L'utente è bloccato. Contatta l'assistenza.");
                }
            }
            else
            {
                Console.WriteLine("Utente non trovato. Riprova.");
            }

            return false; // Autenticazione fallita
        }

        static void MenuPrincipale(bancomatEntities context, Utenti utenteAutenticato)
        {
            bool continua = true;
            

            while (continua)
            {
                Console.WriteLine("\nMenu Principale:");
                Console.WriteLine("1. Versamento");
                Console.WriteLine("2. Mostra Report Saldo");
                Console.WriteLine("3. Prelievo");
                Console.WriteLine("4. Mostra Registro Operazioni");
                Console.WriteLine("5. Logout");
                Console.WriteLine("6. Esci");

                Console.Write("Seleziona un'opzione: ");
                string scelta = Console.ReadLine();

                switch (scelta)
                {
                    case "1":
                        EseguiVersamento(context, utenteAutenticato);
                        break;
                    case "2":
                        MostraReportSaldo(context, utenteAutenticato);
                        break;
                    case "3":
                        EseguiPrelievo(context, utenteAutenticato);
                        break;
                    case "4":
                        MostraRegistroOperazioni(context, utenteAutenticato);
                        break;
                    case "5":
                        continua = false;
                        Console.WriteLine("Logout effettuato.");
                        break;
                    case "6":
                        continua = false;
                        Console.WriteLine("Arrivederci!");
                        break;
                    default:
                        Console.WriteLine("Opzione non valida. Riprova.");
                        break;
                }
            }
        }

        static void EseguiVersamento(bancomatEntities context, Utenti utenteAutenticato)
        {
            Console.Write("Inserisci l'importo da versare: ");
            if (int.TryParse(Console.ReadLine(), out int importo))
            {
                if (importo > 0)
                {
                    // Cerca il conto corrente dell'utente nel database
                    var contoCorrente = context.ContiCorrentes.FirstOrDefault(c => c.IdUtente == utenteAutenticato.Id);

                    if (contoCorrente != null)
                    {
                        // Aggiorna il saldo del conto corrente
                        contoCorrente.Saldo += importo;

                        // Aggiorna la data dell'ultima operazione
                        contoCorrente.DataUltimaOperazione = DateTime.Now;

                        // Registra l'operazione nel database
                        var movimento = new Movimenti
                        {
                            NomeBanca = utenteAutenticato.Banche.Nome,
                            NomeUtente = utenteAutenticato.NomeUtente,
                            Funzionalita = "Versamento",
                            Quantita = importo,
                            DataOperazione = DateTime.Now
                        };

                        context.Movimentis.Add(movimento);
                        context.SaveChanges();

                        Console.WriteLine($"Versamento di {importo} effettuato con successo.");
                    }
                    else
                    {
                        Console.WriteLine("Conto corrente non trovato.");
                    }
                }
                else
                {
                    Console.WriteLine("L'importo deve essere maggiore di zero.");
                }
            }
            else
            {
                Console.WriteLine("Importo non valido.");
            }
        }

        static void MostraReportSaldo(bancomatEntities context, Utenti utenteAutenticato)
        {
            // Cerca il conto corrente dell'utente nel database
            var contoCorrente = context.ContiCorrentes.FirstOrDefault(c => c.IdUtente == utenteAutenticato.Id);

            if (contoCorrente != null)
            {
                Console.WriteLine("\nReport Saldo:");
                Console.WriteLine($"Saldo attuale: {contoCorrente.Saldo}");
                Console.WriteLine($"Data ultimo versamento: {contoCorrente.DataUltimaOperazione}");
                Console.WriteLine($"Data/ora attuale: {DateTime.Now}");
            }
            else
            {
                Console.WriteLine("Conto corrente non trovato.");
            }
        }

        static void EseguiPrelievo(bancomatEntities context, Utenti utenteAutenticato)
        {
            Console.Write("Inserisci l'importo da prelevare: ");
            if (int.TryParse(Console.ReadLine(), out int importo))
            {
                if (importo > 0)
                {
                    // Cerca il conto corrente dell'utente nel database
                    var contoCorrente = context.ContiCorrentes.FirstOrDefault(c => c.IdUtente == utenteAutenticato.Id);

                    if (contoCorrente != null)
                    {
                        if (contoCorrente.Saldo >= importo)
                        {
                            // Esegui il prelievo
                            contoCorrente.Saldo -= importo;

                            // Aggiorna la data dell'ultima operazione
                            contoCorrente.DataUltimaOperazione = DateTime.Now;

                            // Registra l'operazione nel database
                            var movimento = new Movimenti
                            {
                                NomeBanca = utenteAutenticato.Banche.Nome,
                                NomeUtente = utenteAutenticato.NomeUtente,
                                Funzionalita = "Prelievo",
                                Quantita = importo,
                                DataOperazione = DateTime.Now
                            };

                            context.Movimentis.Add(movimento);
                            context.SaveChanges();

                            Console.WriteLine($"Prelievo di {importo} effettuato con successo.");
                        }
                        else
                        {
                            Console.WriteLine("Saldo insufficiente per il prelievo.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Conto corrente non trovato.");
                    }
                }
                else
                {
                    Console.WriteLine("L'importo deve essere maggiore di zero.");
                }
            }
            else
            {
                Console.WriteLine("Importo non valido.");
            }
        }

        static void MostraRegistroOperazioni(bancomatEntities context, Utenti utenteAutenticato)
        {
            // Verifica se la banca dell'utente ha la funzionalità "Registro Operazioni"
            var banca = context.Banches.FirstOrDefault(b => b.Id == utenteAutenticato.IdBanca);

            if (banca != null && HaFunzionalitaRegistroOperazioni(context, banca))
            {
                // Cerca tutte le operazioni dell'utente nel database
                var operazioni = context.Movimentis
                    .Where(m => m.NomeBanca == utenteAutenticato.Banche.Nome && m.NomeUtente == utenteAutenticato.NomeUtente)
                    .ToList();

                if (operazioni.Count > 0)
                {
                    Console.WriteLine("\nRegistro delle Operazioni:");
                    foreach (var operazione in operazioni)
                    {
                        Console.WriteLine($"Data operazione: {operazione.DataOperazione}");
                        Console.WriteLine($"Utente: {operazione.NomeUtente}");
                        Console.WriteLine($"Operazione: {operazione.Funzionalita}");
                        Console.WriteLine($"Importo: {operazione.Quantita}");
                        Console.WriteLine("--------------------------");
                    }
                }
                else
                {
                    Console.WriteLine("Nessuna operazione registrata.");
                }
            }
            else
            {
                Console.WriteLine("La tua banca non supporta la funzionalità Registro Operazioni.");
            }
        }

        static bool HaFunzionalitaRegistroOperazioni(bancomatEntities context, Banche banca)
        {
            // Verifica se la banca ha la funzionalità "Registro Operazioni"
            return context.Banche_Funzionalita
                .Any(bf => bf.IdBanca == banca.Id && bf.Funzionalita.Nome == "Registro Operazioni");
        }





    }
}
