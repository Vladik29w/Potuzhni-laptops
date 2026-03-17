import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LaptopDetails } from './details.component';

describe('LaptopDetails', () => {
  let component: LaptopDetails;
  let fixture: ComponentFixture<LaptopDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LaptopDetails]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LaptopDetails);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
