import { Component, inject, DestroyRef } from '@angular/core';
import { OrderService } from '../../services/order.service';
import { CartService } from '../../services/cart.service';
import { NovaPostService } from '../../services/nova-post.service';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { RouterLink, RouterOutlet } from '@angular/router';
import { CartDTO } from '../../DTO/cart-dto';
import { NpSettlementAddress, NpWarehouse } from '../../DTO/novapost-dto';
import { PayEnum, DeliveryEnum } from '../../DTO/order-dto';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, Observable, distinctUntilChanged, filter, of, switchMap, catchError } from 'rxjs';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-order.component',
  standalone: true,
  imports: [ReactiveFormsModule, RouterOutlet, RouterLink, AsyncPipe],
  templateUrl: './order.component.html',
})
export class OrderComponent {
  private _orderService = inject(OrderService);
  private _cartService = inject(CartService);
  private _novaPostService = inject(NovaPostService);
  private _destroyRef = inject(DestroyRef);

  orderForm = new FormGroup({
    pay: new FormControl<PayEnum>(PayEnum.Unknown, { nonNullable: true }),
    delivery: new FormControl<DeliveryEnum>(DeliveryEnum.Unknown, { nonNullable: true }),
    phone: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    email: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    citySearch: new FormControl<string>('', { nonNullable: true }),
    cityRef: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    warehouseSearch: new FormControl<string>('', { nonNullable: true }),
    warehouseRef: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] })
  });

  cart?: CartDTO;

  paymentOptions = [
    { id: PayEnum.Cash, name: 'Cash' },
    { id: PayEnum.Card, name: 'Card' },
    { id: PayEnum.GooglePay, name: 'Google Pay' },
    { id: PayEnum.ApplePay, name: 'Apple Pay' }
  ];

  deliveryOptions = [
    { id: DeliveryEnum.PickUp, name: 'Pickup' },
    { id: DeliveryEnum.NovaPost, name: 'Nova Post' }
  ];

  cities: Observable<NpSettlementAddress[]> = this.orderForm.controls.citySearch.valueChanges.pipe(
    debounceTime(300),
    distinctUntilChanged(),
    filter(queue => queue.length >= 3),
    switchMap(queue => this._novaPostService.getCities(queue).pipe(
      catchError(() => of([]))
    ))
  );

  warehouses: Observable<NpWarehouse[]> = this.orderForm.controls.warehouseSearch.valueChanges.pipe(
    debounceTime(300),
    distinctUntilChanged(),
    switchMap(queue => {
      let ref = this.orderForm.controls.cityRef.getRawValue();
      if (!ref) return of([]);

      return this._novaPostService.getWarehouses(ref, queue).pipe(
        catchError(() => of([]))
      );
    })
  );

  constructor() {
    this._cartService.getCart()
      .pipe(takeUntilDestroyed())
      .subscribe((data) => this.cart = data);
  }

  selectCity(city: NpSettlementAddress) {
    this.orderForm.patchValue({
      citySearch: city.Present,
      cityRef: city.Ref,
      warehouseSearch: '',
      warehouseRef: ''
    });
    this.orderForm.controls.warehouseSearch.setValue('');
  }

  selectWarehouse(warehouse: NpWarehouse) {
    this.orderForm.patchValue({
      warehouseSearch: warehouse.Description,
      warehouseRef: warehouse.Ref
    });
  }

  createOrder() {
    if (this.orderForm.invalid) {
      alert('Please fill in all required fields');
      return;
    }

    const { pay, delivery, phone, email, cityRef, citySearch, warehouseRef, warehouseSearch } = this.orderForm.getRawValue();

    this._orderService.createOrder({
      cartId: this._cartService.cartId,
      payMethod: pay,
      deliveryMethod: delivery,
      phoneNumber: phone,
      email: email,
      deliveryCityRef: cityRef,
      deliveryCityName: citySearch,
      deliveryWarehouseRef: warehouseRef,
      deliveryWarehouseName: warehouseSearch
    }).pipe(
      takeUntilDestroyed(this._destroyRef)
    ).subscribe({
      next: (orderId) => {
        alert(`Your order ID: ${orderId}`);
        this.orderForm.reset();
      },
      error: (error) => {
        console.error('Order creation failed:', error);
      }
    });
  }

  public get novaPostEnumId(): number {
    return DeliveryEnum.NovaPost;
  }
}
