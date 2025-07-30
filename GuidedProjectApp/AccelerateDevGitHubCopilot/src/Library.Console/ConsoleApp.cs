using Library.ApplicationCore;
using Library.ApplicationCore.Entities;
using Library.ApplicationCore.Enums;
using Library.Console;
using Library.Infrastructure.Data;

public class ConsoleApp
{
    ConsoleState _currentState = ConsoleState.PatronSearch;

    List<Patron> matchingPatrons = new List<Patron>();

    Patron? selectedPatronDetails = null;
    Loan selectedLoanDetails = null!;

    IPatronRepository _patronRepository;
    ILoanRepository _loanRepository;
    ILoanService _loanService;
    IPatronService _patronService;
    private readonly JsonData _jsonData;

    public ConsoleApp(
        ILoanService loanService,
        IPatronService patronService,
        IPatronRepository patronRepository,
        ILoanRepository loanRepository,
        JsonData jsonData
    )
    {
        _patronRepository = patronRepository;
        _loanRepository = loanRepository;
        _loanService = loanService;
        _patronService = patronService;
        _jsonData = jsonData;
    }

    public async Task Run()
    {
        while (true)
        {
            switch (_currentState)
            {
                case ConsoleState.PatronSearch:
                    _currentState = await PatronSearch();
                    break;
                case ConsoleState.PatronSearchResults:
                    _currentState = await PatronSearchResults();
                    break;
                case ConsoleState.PatronDetails:
                    _currentState = await PatronDetails();
                    break;
                case ConsoleState.LoanDetails:
                    _currentState = await LoanDetails();
                    break;
            }
        }
    }

    async Task<ConsoleState> PatronSearch()
    {
        string searchInput = ReadPatronName();

        matchingPatrons = await _patronRepository.SearchPatrons(searchInput);

        if (matchingPatrons.Count > 20)
        {
            Console.WriteLine("More than 20 patrons satisfy the search, please provide more specific input...");
            return ConsoleState.PatronSearch;
        }
        else if (matchingPatrons.Count == 0)
        {
            Console.WriteLine("No matching patrons found.");
            return ConsoleState.PatronSearch;
        }

        Console.WriteLine("Matching Patrons:");
        PrintPatronsList(matchingPatrons);
        return ConsoleState.PatronSearchResults;
    }

    static string ReadPatronName()
    {
        string? searchInput = null;
        while (String.IsNullOrWhiteSpace(searchInput))
        {
            Console.Write("Enter a string to search for patrons by name: ");
            searchInput = Console.ReadLine();
        }
        return searchInput;
    }

    static void PrintPatronsList(List<Patron> matchingPatrons)
    {
        int patronNumber = 1;
        foreach (Patron patron in matchingPatrons)
        {
            Console.WriteLine($"{patronNumber}) {patron.Name}");
            patronNumber++;
        }
    }

    async Task<ConsoleState> PatronSearchResults()
    {
        CommonActions options = CommonActions.Select | CommonActions.SearchPatrons | CommonActions.Quit;
        CommonActions action = ReadInputOptions(options, out int selectedPatronNumber);
        if (action == CommonActions.Select)
        {
            if (selectedPatronNumber >= 1 && selectedPatronNumber <= matchingPatrons.Count)
            {
                var selectedPatron = matchingPatrons[selectedPatronNumber - 1];
                selectedPatronDetails = await _patronRepository.GetPatron(selectedPatron.Id);

                Console.WriteLine("Selected Patron Details:");
                Console.WriteLine(selectedPatronDetails);

                return ConsoleState.PatronDetails;
            }
        }
        else if (action == CommonActions.Quit)
        {
            Environment.Exit(0);
        }

        return ConsoleState.PatronSearchResults;
    }

    async Task<ConsoleState> PatronDetails()
    {
        if (selectedPatronDetails == null) return ConsoleState.PatronSearch;

        Console.WriteLine("Patron Details:");
        Console.WriteLine(selectedPatronDetails);

        CommonActions options = CommonActions.Back | CommonActions.ViewLoans | CommonActions.Quit | CommonActions.SearchBooks;
        CommonActions action = ReadInputOptions(options);

        if (action == CommonActions.ViewLoans)
        {
            var loans = await _loanRepository.GetLoansByPatronId(selectedPatronDetails.Id);
            Console.WriteLine("Loans for this patron:");
            foreach (var loan in loans)
            {
                Console.WriteLine(loan);
            }

            return ConsoleState.LoanDetails;
        }
        else if (action == CommonActions.Back)
        {
            return ConsoleState.PatronSearch;
        }
        else if (action == CommonActions.SearchBooks)
        {
            await SearchBooks();
            return ConsoleState.PatronDetails;
        }
        else if (action == CommonActions.Quit)
        {
            Environment.Exit(0);
        }

        return ConsoleState.PatronDetails;
    }

    async Task<ConsoleState> LoanDetails()
    {
        await Task.CompletedTask;

        if (selectedLoanDetails == null) return ConsoleState.PatronSearch;

        Console.WriteLine("Loan Details:");
        Console.WriteLine(selectedLoanDetails);

        CommonActions options = CommonActions.Back | CommonActions.Quit;
        CommonActions action = ReadInputOptions(options);

        if (action == CommonActions.Back)
        {
            return ConsoleState.PatronDetails;
        }
        else if (action == CommonActions.Quit)
        {
            Environment.Exit(0);
        }

        return ConsoleState.LoanDetails;
    }

    async Task<ConsoleState> SearchBooks()
    {
        string? bookTitle = null;
        while (string.IsNullOrWhiteSpace(bookTitle))
        {
            Console.Write("Enter a book title to search for: ");
            bookTitle = Console.ReadLine();
        }

        var (book, bookItem, loan) = await _jsonData.SearchBookByTitle(bookTitle);

        if (book == null)
        {
            Console.WriteLine($"No book found with the title \"{bookTitle}\".");
        }
        else if (bookItem == null)
        {
            Console.WriteLine($"No physical copy found for \"{book.Title}\".");
        }
        else if (loan == null)
        {
            Console.WriteLine($"\"{book.Title}\" is available for loan");
        }
        else
        {
            Console.WriteLine($"\"{book.Title}\" is on loan to another patron. The return due date is {loan.DueDate}");
        }

        return ConsoleState.PatronDetails;
    }

    CommonActions ReadInputOptions(CommonActions options, out int selectedPatronNumber)
    {
        selectedPatronNumber = 0;
        Console.WriteLine("Select an action:");
        foreach (CommonActions option in Enum.GetValues(typeof(CommonActions)))
        {
            if (options.HasFlag(option))
            {
                Console.WriteLine($"- {option}");
            }
        }

        string? input = Console.ReadLine();
        if (int.TryParse(input, out int result) && result >= 1 && result <= matchingPatrons.Count)
        {
            selectedPatronNumber = result;
            return CommonActions.Select;
        }

        return CommonActions.None;
    }

    CommonActions ReadInputOptions(CommonActions options)
    {
        Console.WriteLine("Select an action:");
        foreach (CommonActions option in Enum.GetValues(typeof(CommonActions)))
        {
            if (options.HasFlag(option))
            {
                Console.WriteLine($"- {option}");
            }
        }

        string? input = Console.ReadLine();
        if (Enum.TryParse(input, true, out CommonActions result) && options.HasFlag(result))
        {
            return result;
        }

        return CommonActions.None;
    }
}
