import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient } from '@angular/common/http';
import { User } from '../_models/User';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getUserWithRoles() {
    return this.http.get(this.baseUrl + 'admin/userwithroles');
  }
  updatedUserRoles(user: User, roles) {
    return this.http.post(this.baseUrl + 'admin/editroles/' + user.userName, roles);
  }

  getphotoForModeration() {
    return this.http.get(this.baseUrl + 'admin/photoForModeration/');
  }

  approvePhoto(photoid) {
    return this.http.post(this.baseUrl + 'admin/approvephoto/' + photoid, {});
  }

  rejectphoto(photoid) {
    return this.http.delete(this.baseUrl + 'admin/rejectphoto/' + photoid);
  }
}
