import { TestBed } from '@angular/core/testing';
import { Router, UrlTree, provideRouter } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { customerGuard } from './customer.guard';

describe('customerGuard', () => {
  function runGuard(isAuthenticated: boolean) {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: { isAuthenticated: () => isAuthenticated } }],
    });

    return TestBed.runInInjectionContext(() => customerGuard({} as never, {} as never));
  }

  it('allows navigation when the customer is authenticated', () => {
    expect(runGuard(true)).toBe(true);
  });

  it('redirects to the login page when not authenticated', () => {
    const result = runGuard(false);

    expect(result).not.toBe(true);
    const router = TestBed.inject(Router);
    const expected = router.createUrlTree(['/entrar']);
    expect((result as UrlTree).toString()).toBe(expected.toString());
  });
});
