using Library.ApplicationCore.Entities;

namespace Library.ApplicationCore;

public interface ILoanRepository {
    Task<Loan?> GetLoan(int loanId);
    Task<IEnumerable<object>> GetLoansByPatronId(int id);

    Task UpdateLoan(Loan loan);
}