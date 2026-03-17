import { Component, inject, ChangeDetectorRef, ViewChild, ElementRef, AfterViewInit, OnDestroy, NgZone } from '@angular/core';
import { OrderService } from '../../services/order.service';
import { CartService } from '../../services/cart.service';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { RouterLink, RouterOutlet } from '@angular/router';
import { CartDTO } from '../../DTO/cart-dto';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
declare const google: any;

export enum PayEnum {
  Unknown = 0,
  Cash = 1,
  Card = 2,
  GooglePay = 3,
  ApplePay = 4
}

export enum DeliveryEnum {
  Unknown = 0,
  Pickup = 1,
  NovaPost = 2,
  UkrPost = 3,
}

@Component({
  selector: 'app-order.component',
  standalone: true,
  imports: [ReactiveFormsModule, RouterOutlet, RouterLink],
  templateUrl: './order.component.html',
  styleUrl: './order.component.css',
})
export class OrderComponent implements AfterViewInit, OnDestroy {
  orderForm = new FormGroup({
    pay: new FormControl<PayEnum>(PayEnum.Unknown, { nonNullable: true }),
    delivery: new FormControl<DeliveryEnum>(DeliveryEnum.Unknown, { nonNullable: true }),
    phone: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    email: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    address: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] })
  });
  cart?: CartDTO;
  private _orderService = inject(OrderService);
  private _cartService = inject(CartService);
  private _cdr = inject(ChangeDetectorRef)
  private _ngZone = inject(NgZone);

  @ViewChild('addressContainer') addressContainer!: ElementRef<HTMLDivElement>;
  private autocompleteElement: any; // <-- ДОДАЙ ЦЮ ЗМІННУ
  private gmpSelectHandler: any;

  paymentOptions = [
    { id: PayEnum.Cash, name: 'Cash' },
    { id: PayEnum.Card, name: 'Card' },
    { id: PayEnum.GooglePay, name: 'Google Pay' },
    { id: PayEnum.ApplePay, name: 'Apple Pay' }
  ];
  constructor() {
    this._cartService.getCart()
      .pipe(takeUntilDestroyed())
      .subscribe((data) => this.cart = data);
  }

  deliveryOptions = [
    { id: DeliveryEnum.Pickup, name: 'Pickup' },
    { id: DeliveryEnum.NovaPost, name: 'Nova Post' },
    { id: DeliveryEnum.UkrPost, name: 'UkrPost' }
  ];

  

  ngAfterViewInit() {
    if (typeof google !== 'undefined' && google.maps && google.maps.places) {
      this.initNewAutocomplete();
    }
    else {
      setTimeout(() => this.ngAfterViewInit(), 500)
    }
  }

  initNewAutocomplete() {
    this.autocompleteElement = new google.maps.places.PlaceAutocompleteElement({
      includedRegionCodes: ['UA']
    });
    this.autocompleteElement.style.colorScheme = 'light';
    this.addressContainer.nativeElement.appendChild(this.autocompleteElement);

    this.gmpSelectHandler = async (event: any) => {
      const place = event.placePrediction.toPlace();

      if (place) {
        await place.fetchFields({ fields: ['formattedAddress'] });
        this._ngZone.run(() => {
          this.orderForm.patchValue({ address: place.formattedAddress });
        });
      }
    };

    this.autocompleteElement.addEventListener('gmp-select', this.gmpSelectHandler);
  }
  ngOnDestroy() {
    if (this.autocompleteElement && this.gmpSelectHandler) {
      this.autocompleteElement.removeEventListener('gmp-select', this.gmpSelectHandler);
    }
  }
  createOrder() {
    const { pay, delivery, phone, email, address } = this.orderForm.getRawValue();
    this._orderService.createOrder(pay, delivery, phone, email, address).subscribe(
      orderId => { alert("Your order ID: " + orderId) }
    )
  }
}
