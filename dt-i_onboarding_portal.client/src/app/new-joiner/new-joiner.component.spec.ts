import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewJoinerComponent } from './new-joiner.component';

describe('NewJoinerComponent', () => {
  let component: NewJoinerComponent;
  let fixture: ComponentFixture<NewJoinerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [NewJoinerComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NewJoinerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
