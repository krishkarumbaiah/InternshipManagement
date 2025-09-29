import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-profile-edit',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile-edit.html',
  styleUrls: ['./profile-edit.scss'],
})
export class ProfileEditComponent implements OnInit {
  form: FormGroup;
  loading = false;
  submitting = false;
  selectedFile?: File;
  imagePreview?: string | ArrayBuffer | null;

  constructor(private fb: FormBuilder, private auth: AuthService, private toastr: ToastrService) {
    this.form = this.fb.group({
      fullName: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile() {
    this.loading = true;
    this.auth.getProfile().subscribe({
      next: (res) => {
        this.form.patchValue({ fullName: res.fullName });

        if (res.profilePhoto) {
          
          console.log('ProfileEdit.loadProfile -> server profilePhoto:', res.profilePhoto);
          this.auth.setProfilePhoto(res.profilePhoto);
          
          this.imagePreview = this.auth.getProfilePhoto();
          console.log('ProfileEdit.loadProfile -> preview set to', this.imagePreview);
        } else {
          // fallback to default avatar
          this.auth.setProfilePhoto('/assets/default-avatar.png');
          this.imagePreview = this.auth.getProfilePhoto();
        }

        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load profile', err);
        this.toastr.error('Failed to load profile');
        this.loading = false;
      }
    });
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      this.selectedFile = undefined;
      return;
    }

    const file = input.files[0];
    const allowed = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowed.includes(file.type)) {
      this.toastr.error('Only JPG/PNG/WEBP allowed');
      input.value = '';
      return;
    }

    const maxBytes = 2 * 1024 * 1024;
    if (file.size > maxBytes) {
      this.toastr.error('Image too large (max 2MB)');
      input.value = '';
      return;
    }

    this.selectedFile = file;

    const reader = new FileReader();
    reader.onload = () => (this.imagePreview = reader.result);
    reader.readAsDataURL(file);
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toastr.warning('Please fill the name');
      return;
    }

    const formData = new FormData();
    formData.append('FullName', this.form.get('fullName')?.value);
    if (this.selectedFile) {
      formData.append('ProfilePhoto', this.selectedFile, this.selectedFile.name);
    }

    this.submitting = true;
    this.auth.updateProfile(formData).subscribe({
      next: (res) => {
        this.toastr.success('Profile updated');

        if (res.profilePhoto) {
          console.log('ProfileEdit.submit -> server returned profilePhoto:', res.profilePhoto);
          this.auth.setProfilePhoto(res.profilePhoto);

          this.imagePreview = this.auth.getProfilePhoto();
          console.log('ProfileEdit.submit -> preview updated to', this.imagePreview);
        } else {
         
          this.auth.setProfilePhoto('/assets/default-avatar.png');
          this.imagePreview = this.auth.getProfilePhoto();
        }

        
        this.selectedFile = undefined;
     
        this.loadProfile();

        this.submitting = false;
      },
      error: (err) => {
        console.error(err);
        this.toastr.error(err?.error?.message ?? 'Failed to update profile');
        this.submitting = false;
      }
    });
  }
}
