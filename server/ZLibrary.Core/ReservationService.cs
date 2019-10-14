using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZLibrary.API;
using ZLibrary.Model;
using ZLibrary.Persistence;

namespace ZLibrary.Core
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository reservationRepository;
        private readonly ILoanService loanService;

        public ReservationService(IReservationRepository reservationRepository, ILoanService loanService)
        {
            this.reservationRepository = reservationRepository;
            this.loanService = loanService;
        }

        public async Task<IList<Reservation>> FindAll()
        {
            return await reservationRepository.FindAll();
        }

        public async Task<Reservation> FindById(long id)
        {
            if (id <= 0)
            {
                return null;
            }
            return await reservationRepository.FindById(id);
        }

        public async Task<IList<Reservation>> FindByBookId(long bookId)
        {
            return await reservationRepository.FindByBookId(bookId);
        }

        public async Task<IList<Reservation>> FindBookReservations(long bookId)
        {
            if (bookId <= 0)
            {
                return null;
            }
            var loans = await loanService.FindByBookId(bookId);
            var loansBorrowed = loans.Where(l => l.Status != LoanStatus.Returned).ToList();
            var reservations = await reservationRepository.FindByBookId(bookId);
            var reservationApprovedIds = reservations.Where(r => r.Reason.Status == ReservationStatus.Approved).Select(r => r.Id).ToList();

            return loansBorrowed.Where(l => reservationApprovedIds.Contains(l.Reservation.Id)).Select(l => l.Reservation).ToList();
        }

        public async Task<IList<Reservation>> FindByStatus(ReservationStatus reservationStatus)
        {
            return await reservationRepository.FindByStatus(reservationStatus);
        }

        public async Task<IList<Reservation>> FindByUserId(long userId)
        {
            if (userId <= 0)
            {
                return null;
            }
            return await reservationRepository.FindByUserId(userId);
        }

        public async Task ApproveReservation(Reservation reservation, Book book)
        {
            if(reservation.IsApproved)
            {
                return;
            }

            if (!(reservation.IsRequested || reservation.IsWaiting))
            {
                throw new ReservationApprovedException("O Status da reserversa precisa ser Solicitada ou Aguardando.");
            }

            var loans = await loanService.FindByBookId(reservation.BookId);
            var loanBorrowedByUser = loans.SingleOrDefault(l => l.Reservation.User.Id == reservation.User.Id && l.Status == LoanStatus.Borrowed);
            if (loanBorrowedByUser != null)
            {
                await CreateLoan(reservation);
                await loanService.ReturnLoan(loanBorrowedByUser.Id);
            }
            else if (book.CanApproveLoan(loans))
            {
                await CreateLoan(reservation);
            }
        }

        public async Task QueueReservation(Reservation reservation)
        {
            if(reservation.IsWaiting)
            {
                return;
            }

            if (!reservation.IsRequested)
            {
                throw new ReservationApprovedException("O Status da reserversa precisa ser Solicitada");
            }

            reservation.Reason.Status = ReservationStatus.Waiting;
            await reservationRepository.Update(reservation);
        }

        public async Task ReturnReservation(Reservation reservation, Book book)
        {
            if(reservation.IsReturned)
            {
                return;
            }

            if (!reservation.IsApproved)
            {
                throw new ReservationApprovedException("O Status da reserversa precisa ser Aprovada.");
            }

            var loans = await loanService.FindByBookId(reservation.BookId);
            var loanBorrowedByUser = loans.SingleOrDefault(loan => loan.Reservation.User.Id == reservation.User.Id && loan.Status == LoanStatus.Borrowed);
            if (loanBorrowedByUser == null)
            {
                throw new InvalidLoanException("Status inválido do emprestimo");
            }
            await loanService.ReturnLoan(loanBorrowedByUser.Id);
            reservation.Reason.Status = ReservationStatus.Returned;
            await reservationRepository.Update(reservation);
        }

        public async Task CancelReservation(Reservation reservation)
        {
            if(reservation.IsCanceled)
            {
                return;
            }

            if (!reservation.IsRequested && !reservation.IsWaiting)
            {
                throw new ReservationApprovedException("O Status da reserversa precsisa ser Solicitado ou na Lista de Espera.");
            }
            reservation.Reason.Status = ReservationStatus.Canceled;
            await reservationRepository.Update(reservation);
        }

        public async Task<Reservation> Order(Book book, User user)
        {
            return await reservationRepository.Save(new Reservation(book.Id, user));
        }

        public async Task OrderNext(long bookId)
        {
            var reservations = await reservationRepository.FindByBookId(bookId);
            var firstReservationWainting = reservations.OrderBy(r => r.StartDate).FirstOrDefault(r => r.Reason.Status == ReservationStatus.Waiting);

            if (firstReservationWainting != null)
            {
                await CreateLoan(firstReservationWainting);
            }
        }

        private async Task CreateLoan(Reservation reservation)
        {
            reservation.Reason.Status = ReservationStatus.Approved;
            await reservationRepository.Update(reservation);
            var loan = new Loan(reservation);
            await loanService.Create(loan);
        }
    }
}