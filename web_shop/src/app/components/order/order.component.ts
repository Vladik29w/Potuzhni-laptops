import { Component, inject, DestroyRef } from '@angular/core';
import { OrderService } from '../../services/order.service';
import { CartService } from '../../services/cart.service';
import { NovaPostService } from '../../services/nova-post.service';
import { ReactiveFormsModule, FormGroup, FormControl, FormBuilder, Validators } from '@angular/forms';
import { RouterLink, RouterOutlet } from '@angular/router';
import { CartDTO } from '../../DTO/cart-dto';
import { NpSettlementAddress, NpWarehouse } from '../../DTO/novapost-dto';
import { OrderResponce, CreateOrderDTO, PayEnum, DeliveryEnum } from '../../DTO/order-dto';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, Observable, distinctUntilChanged, filter, of, switchMap, catchError } from 'rxjs';
import { AsyncPipe, DOCUMENT } from '@angular/common';
//TODO
//баг з автокомплітом НП
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
  private fb = inject(FormBuilder).nonNullable;
  private _document = inject(DOCUMENT);

 orderForm = this.fb.group({
    pay: [PayEnum.Unknown],
    delivery: [DeliveryEnum.Unknown],
    
    customerInfo: this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(32)]],
      lastName: ['', [Validators.required, Validators.maxLength(32)]],
      middleName: [''],
      phoneNumber: ['', [Validators.required, Validators.pattern(/^\+?\d{10,15}$/)]],
      email: ['', [Validators.email]]
    }),

    deliveryDetails: this.fb.group({
      citySearch: [''],
      cityRef: ['', [Validators.required]],
      warehouseSearch: [''],
      warehouseRef: ['', [Validators.required]]
    })
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

  cities: Observable<NpSettlementAddress[]> = this.orderForm.controls.deliveryDetails.controls.citySearch.valueChanges.pipe(
    debounceTime(300),
    distinctUntilChanged(),
    filter(queue => queue.length >= 3),
    switchMap(queue => this._novaPostService.getCities(queue).pipe(
      catchError(() => of([]))
    ))
  );

  warehouses: Observable<NpWarehouse[]> = this.orderForm.controls.deliveryDetails.controls.warehouseSearch.valueChanges.pipe(
    debounceTime(300),
    distinctUntilChanged(),
    switchMap(queue => {
      let ref = this.orderForm.controls.deliveryDetails.controls.cityRef.value;
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
        const deliveryDetailsGroup = this.orderForm.controls.deliveryDetails;
        const cityRefControl = deliveryDetailsGroup.controls.cityRef;
        const warehouseRefControl = deliveryDetailsGroup.controls.warehouseRef;

        if (deliveryMethod === DeliveryEnum.NovaPost) {
          // Required for Nova Post
          cityRefControl?.setValidators([Validators.required]);
          warehouseRefControl?.setValidators([Validators.required]);
        } else if (deliveryMethod === DeliveryEnum.PickUp) {
          // Not required for Pickup
          cityRefControl?.setValidators([]);
          warehouseRefControl?.setValidators([]);
          // Clear the values
          cityRefControl?.setValue('');
          warehouseRefControl?.setValue('');
          deliveryDetailsGroup?.patchValue({
            citySearch: '',
            warehouseSearch: ''
          });
        }

        cityRefControl?.updateValueAndValidity();
        warehouseRefControl?.updateValueAndValidity();
      });
  }

  selectCity(city: NpSettlementAddress) {
    this.orderForm.patchValue({
      deliveryDetails: {
        citySearch: city.Present,
        cityRef: city.SettlementRef || city.Ref,
        warehouseSearch: '',
        warehouseRef: ''
      }
    });
  }

  selectWarehouse(warehouse: NpWarehouse) {
    this.orderForm.patchValue({
      deliveryDetails: {
        warehouseSearch: warehouse.Description,
        warehouseRef: warehouse.Ref
      }
    });
  }

  createOrder() {
    if (this.orderForm.invalid) {
      alert('Please fill in all required fields');
      return;
    }

    const formValue = this.orderForm.getRawValue();

    const orderData: CreateOrderDTO = {
      cartId: this._cartService.cartId,
      payMethod: formValue.pay,
      deliveryMethod: formValue.delivery,
      customerInfo: {
        firstName: formValue.customerInfo.firstName,
        middleName: formValue.customerInfo.middleName,
        lastName: formValue.customerInfo.lastName,
        phoneNumber: formValue.customerInfo.phoneNumber,
        email: formValue.customerInfo.email ?? undefined
      },
      ...(formValue.delivery === DeliveryEnum.NovaPost && {
        deliveryCityRef: formValue.deliveryDetails.cityRef,
        deliveryCityName: formValue.deliveryDetails.citySearch,
        deliveryWarehouseRef: formValue.deliveryDetails.warehouseRef,
        deliveryWarehouseName: formValue.deliveryDetails.warehouseSearch
      })
    };

    this._orderService.createOrder(orderData).pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe({
      next: (res: OrderResponce) => {
          console.log(`Order created: ${res.orderId}`);
        if (formValue.pay === PayEnum.Online && res.paymentUrl) {
          this._document.location.href = res.paymentUrl;
        } else {
          alert('Order created successfully');
          this._document.location.href = "/";
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
