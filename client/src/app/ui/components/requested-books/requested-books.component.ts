import { ReservationStatus } from './../../../model/reservation-status';
import { Component, OnInit, Input, ViewEncapsulation } from '@angular/core';
import { ReservationService } from '../../../service/reservation.service';
import index from '@angular/cli/lib/cli';
import { BsModalRef } from 'ngx-bootstrap/modal/bs-modal-ref.service';
import { ToastMediator } from '../../mediators/toast.mediator';
import { LoaderMediator } from '../../mediators/loader.mediator';
import { Order } from '../../../model/order';
import { ConfirmMediator } from '../../mediators/confirm.mediator';

@Component({
    selector: 'zli-requested-books',
    templateUrl: './requested-books.component.html',
    styleUrls: ['./requested-books.component.scss'],
    encapsulation: ViewEncapsulation.Emulated
})
export class RequestedBooksComponent implements OnInit {
    constructor(private reservationService: ReservationService,
        private toastMediator: ToastMediator,
        private confirmMediator: ConfirmMediator,
        private loaderMediator: LoaderMediator) {
    }

    @Input() public orders: Order[];
    public modalControl: BsModalRef;
    public reservationStatus: ReservationStatus;

    ngOnInit() {
        this.showRequestedReservations();
    }

    public showRequestedReservations() {
        this.reservationService.findOrdersByStatus(ReservationStatus.Requested)
            .subscribe((orders: Order[]) => {
                this.orders = orders;
            });
        this.orders = null;
    }

    public acceptReservation(order: Order) {
        this.confirmMediator.showDialog('APROVAR', 'Deseja aprovar está reserva?').subscribe(r => {
            if (r) {
                this.loaderMediator.execute(
                    this.reservationService.approve(order.reservation.id)
                        .subscribe(() => {
                            this.showRequestedReservations();
                        }
                            , error => {
                                this.toastMediator.show(`Erro ao aprovar a reserva: ${error}`);
                            }
                        )
                );
            }
        });
    }

    public rejectReservation(order: Order) {
        this.confirmMediator.showDialog('RECUSAR', 'Deseja recusar está reserva?').subscribe(r => {
            if (r) {
                this.loaderMediator.execute(
                    this.reservationService.reject(order.reservation.id)
                        .subscribe(() => {
                            this.showRequestedReservations();
                        }
                            , error => {
                                this.toastMediator.show(`Erro ao recusar a reserva: ${error}`);
                            }
                        )
                );
            }
        });
    }


    public close(): void {
        this.modalControl.hide();
    }
}