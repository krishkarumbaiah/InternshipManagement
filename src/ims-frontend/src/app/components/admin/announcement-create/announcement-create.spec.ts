import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AnnouncementCreate } from './announcement-create';

describe('AnnouncementCreate', () => {
  let component: AnnouncementCreate;
  let fixture: ComponentFixture<AnnouncementCreate>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AnnouncementCreate]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AnnouncementCreate);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
