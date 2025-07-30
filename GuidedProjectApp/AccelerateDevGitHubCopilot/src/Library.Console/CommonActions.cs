namespace Library.Console;

[Flags]
public enum CommonActions
{
    None = 0,
    Repeat = 1,
    Select = 2,
    Quit = 4,
    SearchPatrons = 8,
    RenewPatronMembership = 16,
    ReturnLoanedBook = 32,
    ExtendLoanedBook = 64,
    SearchBooks = 128,
    ViewLoans = 256,
    Back = 512
}
