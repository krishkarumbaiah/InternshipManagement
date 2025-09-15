import { TestBed } from '@angular/core/testing';

import { Batch } from './batch';

describe('Batch', () => {
  let service: Batch;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Batch);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
