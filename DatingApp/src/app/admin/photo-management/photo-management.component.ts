import { Component, OnInit } from '@angular/core';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: any[];

  constructor(private adminService: AdminService) {}

  ngOnInit() {
    this.getPhotoForModeration();
  }

  getPhotoForModeration() {
    this.adminService.getphotoForModeration().subscribe(
      (photo: any[] = []) => {
        this.photos = photo;
      },
      error => console.log(error)
    );
  }

  approvePhoto(photoid) {
    this.adminService.approvePhoto(photoid).subscribe(
      () => {
        this.photos.splice(this.photos.findIndex(p => p.id == photoid), 1);
      },
      error => console.log(error)
    );
  }

  rejectphoto(photoid) {
    this.adminService.rejectphoto(photoid).subscribe(
      () => {
        this.photos.splice(this.photos.findIndex(p => p.id == photoid), 1);
      },
      error => console.log(error)
    );
  }
}
