import { Component, computed, signal, effect } from '@angular/core';
import { LaptopService } from '../services/laptop.service';
import { LaptopMainDTO, PagedResultDTO } from '../DTO/laptop-dto';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { rxResource } from '@angular/core/rxjs-interop';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-main.component',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './main.component.html',
  styles: ``,
})
export class MainComponent {
  page = signal(1);
  pageSize = 12;
  searchQuery = signal('');
  private searchSubject = new Subject<string>();

  laptopsResource = rxResource<PagedResultDTO<LaptopMainDTO>, { page: number; pageSize: number }>({
    params: () => ({ page: this.page(), pageSize: this.pageSize }),
    stream: ({ params }) => this.laptopService.getAllLaptops(params.page, params.pageSize)
  });

  filteredLaptops = computed(() => {
    const laptops = this.laptopsResource.value()?.items ?? [];
    const text = this.searchQuery().trim().toLowerCase();

    if (!text) {
      return laptops;
    }

    return laptops.filter(laptop => laptop.name.toLowerCase().includes(text));
  });

  totalCount = computed(() => this.laptopsResource.value()?.totalCount ?? 0);
  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));

  constructor(private laptopService: LaptopService) {
    effect(() => {
      this.searchSubject
        .pipe(
          debounceTime(200),
          distinctUntilChanged()
        )
        .subscribe(value => {
          this.searchQuery.set(value);
        });
    });
  }

  setSearchQuery(value: string) {
    this.searchSubject.next(value);
  }

  previousPage() {
    if (this.page() > 1) {
      this.page.update(v => v - 1);
    }
  }

  nextPage() {
    if (this.page() < this.totalPages()) {
      this.page.update(v => v + 1);
    }
  }
}
