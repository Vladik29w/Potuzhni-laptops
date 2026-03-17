import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LaptopService } from '../../services/laptop.service'
import { CommonModule } from '@angular/common';
import { CartService } from '../../services/cart.service'
import { LaptopDetailsDTO } from '../../DTO/laptop-dto';

@Component({
  selector: 'app-laptop-details',
  imports: [CommonModule],
  templateUrl: './details.component.html',
  standalone: true,
  styles: ``,
})
export class LaptopDetails implements OnInit {
  laptop: LaptopDetailsDTO | null = null;
  constructor
    (
      private route: ActivatedRoute,
      private laptopService: LaptopService,
      private cartService: CartService
    ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      let id = params.get('id')!;
      this.laptopService.getLaptopById(id).subscribe({
        next: (foundLaptop) => {
          if (foundLaptop) {
            this.laptop = foundLaptop;
          }
        }
      })
    })
    }
  addProduct(): void {
    if (this.laptop) {
      this.cartService.addToCart(this.laptop.id).subscribe({
        next: () => {
          this.cartService.openCart();
        },
        error: (err) => {
          console.error('error while trying to add', err);
        }
      });
    }
  }
}
