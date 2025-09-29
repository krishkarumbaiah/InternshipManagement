import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AnnouncementList } from './announcement-list';

describe('AnnouncementList', () => {
  let component: AnnouncementList;
  let fixture: ComponentFixture<AnnouncementList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AnnouncementList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AnnouncementList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
