import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { CardModule } from 'primeng/card';
import { AuthStore } from '../../../core/stores/auth.store';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, ButtonModule, InputTextModule, PasswordModule, CardModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly authStore = inject(AuthStore);

  protected readonly email = signal('');
  protected readonly password = signal('');
  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected handleSubmit(): void {
    if (!this.email() || !this.password()) {
      this.errorMessage.set('oops! fill in both fields bestie 💌');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.authStore
      .login(this.email(), this.password())
      .finally(() => this.isSubmitting.set(false));
  }

  protected handleKeycloakLogin(): void {
    this.authStore.loginWithKeycloak();
  }
}
