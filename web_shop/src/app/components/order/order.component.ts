import { Component, inject, DestroyRef } from '@angular/core';
import { OrderService } from '../../services/order.service';
import { CartService } from '../../services/cart.service';
import { NovaPostService } from '../../services/nova-post.service';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { RouterLink, RouterOutlet } from '@angular/router';
import { CartDTO } from '../../DTO/cart-dto';
import { NpSettlementAddress, NpWarehouse } from '../../DTO/novapost-dto';
import { OrderResponce, CreateOrderDTO, PayEnum, DeliveryEnum } from '../../DTO/order-dto';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, Observable, distinctUntilChanged, filter, of, switchMap, catchError } from 'rxjs';
import { AsyncPipe, DOCUMENT } from '@angular/common';
//TODO
//баг з автокомплітом НП
//баг з оплатою кешом
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
  private _document = inject(DOCUMENT);

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
    { id: PayEnum.Online, name: 'Online' }
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

    // Handle dynamic validation for delivery fields
    this.orderForm.controls.delivery.valueChanges
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((deliveryMethod) => {
        const cityRefControl = this.orderForm.controls.cityRef;
        const warehouseRefControl = this.orderForm.controls.warehouseRef;

        if (deliveryMethod === DeliveryEnum.NovaPost) {
          // Required for Nova Post
          cityRefControl.setValidators([Validators.required]);
          warehouseRefControl.setValidators([Validators.required]);
        } else if (deliveryMethod === DeliveryEnum.PickUp) {
          // Not required for Pickup
          cityRefControl.setValidators([]);
          warehouseRefControl.setValidators([]);
          // Clear the values
          cityRefControl.setValue('');
          warehouseRefControl.setValue('');
        }

        cityRefControl.updateValueAndValidity();
        warehouseRefControl.updateValueAndValidity();
      });
  }

  selectCity(city: NpSettlementAddress) {
    this.orderForm.patchValue({
      citySearch: city.Present,
      cityRef: city.SettlementRef || city.Ref,
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

    const orderData: CreateOrderDTO = {
      cartId: this._cartService.cartId,
      payMethod: pay,
      deliveryMethod: delivery,
      phoneNumber: phone,
      email: email,
      ...(delivery === DeliveryEnum.NovaPost && {
        deliveryCityRef: cityRef,
        deliveryCityName: citySearch,
        deliveryWarehouseRef: warehouseRef,
        deliveryWarehouseName: warehouseSearch
      })
    };

    this._orderService.createOrder(orderData).pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe({
      next: (res: OrderResponce) => {
          console.log(`Order created: ${res.orderId}`);
          this._document.location.href = "/";
        if (pay === PayEnum.Online && res.paymentUrl) {
          this._document.location.href = res.paymentUrl;
        } else {
          alert('Order created successfully');
          this.orderForm.reset();
        }
      },
      error: (error) => {
        console.error('Order creation failed:', error);
      }
    });
  }

  public get novaPostEnumId(): number {
    return DeliveryEnum.NovaPost;
  }

  public get onlinePayment(): number {
    return PayEnum.Online;
  }

  public get isOnlinePayment(): boolean {
    return this.orderForm.get('pay')?.value === PayEnum.Online;
  }

  public get buttonText(): string {
    return this.isOnlinePayment ? 'Pay now' : 'Create order';
  }
}
