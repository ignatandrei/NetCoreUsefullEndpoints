import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoadHttpComponent } from './load-http.component';

describe('LoadHttpComponent', () => {
  let component: LoadHttpComponent;
  let fixture: ComponentFixture<LoadHttpComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoadHttpComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoadHttpComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
