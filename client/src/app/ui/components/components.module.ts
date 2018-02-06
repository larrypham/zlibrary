import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { BookComponent } from './book/book.component';
import { BookPopupComponent } from './book-popup/book-popup.component';
import { SharedModule } from '../../shared.module';
import { NavbarComponent } from './navbar/navbar.component';
import { LoadingOverlayComponent } from './loading-overlay/loading-overlay.component';
import { ToastComponent } from './toast/toast.component';
import { ReservationHistoryComponent } from './reservation-history/reservation-history.component';

@NgModule({
    declarations: [
        BookComponent,
        BookPopupComponent,
        NavbarComponent,
        LoadingOverlayComponent,
        ToastComponent,
        ReservationHistoryComponent
    ],
    exports: [
        BookComponent,
        BookPopupComponent,
        NavbarComponent,
        LoadingOverlayComponent,
        ToastComponent,
        ReservationHistoryComponent
    ],
    entryComponents: [
        ToastComponent,
        ReservationHistoryComponent
    ],
    imports: [
        BrowserModule,
        SharedModule
    ],
    providers: []
})
export class ComponentsModule {
}