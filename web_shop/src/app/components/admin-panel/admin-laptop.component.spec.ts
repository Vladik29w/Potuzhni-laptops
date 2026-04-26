import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminLaptopComponent } from './admin-laptop.component';

describe('AdminLaptopComponent', () => {
  let component: AdminLaptopComponent;
  let fixture: ComponentFixture<AdminLaptopComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminLaptopComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminLaptopComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
