import { Component, OnInit } from '@angular/core';
import { LaptopService } from '../services/laptop.service';
import { LaptopMainDTO } from '../DTO/laptop-dto';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { LaptopDetails } from '../components/laptop-details/details.component';
import { FormControl, ReactiveFormsModule } from '@angular/forms'
import { distinctUntilChanged, debounceTime } from 'rxjs';

@Component({
  selector: 'app-main.component',
  standalone: true,
  imports: [CommonModule, RouterModule, LaptopDetails, ReactiveFormsModule],
  templateUrl: './main.component.html',
  styles: ``,
})
export class MainComponent implements OnInit {
  laptops: LaptopMainDTO[] = [];
  filteredLaptops: LaptopMainDTO[] = [];
  constructor(private laptopService: LaptopService) {}

  ngOnInit(): void {
    this.laptopService.getAllLaptops().subscribe({
      next: (data) => {
        this.laptops = data;
        this.filteredLaptops = data;
      }
    });   

    this.searchForm.valueChanges.pipe(
      distinctUntilChanged(),
      debounceTime(250)
    )
      .subscribe(
        (value: string | null) => {
          this.filteredResults(value || '')
        }
      )
  }
  
  searchForm = new FormControl('');

  filteredResults(text: string) {
    if (!text) {
      this.filteredLaptops = this.laptops;
      return;
    }
    this.filteredLaptops = this.laptops.filter(laptop =>
      laptop?.name.toLocaleLowerCase().includes(text.toLowerCase())
    )
  }
}
