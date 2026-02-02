import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { NewJoinerService, CreateNewJoinerDto } from './new-joiner.service';

@Component({
  selector: 'app-new-joiner',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './new-joiner.component.html',
  styleUrls: ['./new-joiner.component.css']
})
export class NewJoinerComponent {
  dto: CreateNewJoinerDto = {
    fullName: '',
    email: '',
    department: '',
    managerName: '',
    startDate: ''
  };

  isSubmitting = false;
  error = '';
  success = '';

  constructor(private service: NewJoinerService) {}

  resetForm(form: NgForm) {
    form.resetForm();
    this.error = '';
    this.success = '';
    this.isSubmitting = false;
  }

  submit(form: NgForm) {
    if (this.isSubmitting || form.invalid) return;

    this.error = '';
    this.success = '';
    this.isSubmitting = true;

    // Ensure a proper ISO date string if backend expects it
    const payload: CreateNewJoinerDto = {
      ...this.dto,
      startDate: new Date(this.dto.startDate).toISOString()
    };

    this.service.createNewJoiner(payload).subscribe({
      next: () => {
        //  Show a clean success message ("OK") instead of raw JSON
        this.success = ' New joiner created successfully. ';
        this.isSubmitting = false;
        // Optionally clear the form
        form.resetForm();
      },
      error: (err) => {
        console.error('Create new joiner error:', err);
        const status = err?.status;
        if (status === 401) {
          this.error = 'Unauthorized. Please login again or ensure you have Admin privileges.';
        } else if (status === 0) {
          this.error = 'Cannot reach server. Check if API is running and CORS is enabled.';
        } else {
          // If backend returns { message, advice }, show those instead of JSON
          const message = err?.error?.message || 'Failed to create joiner.';
          const advice = err?.error?.advice ? ` ${err.error.advice}` : '';
          this.error = `${message}${advice} (status ${status ?? 'unknown'})`;
        }
        this.isSubmitting = false;
      }
    });
  }
}
