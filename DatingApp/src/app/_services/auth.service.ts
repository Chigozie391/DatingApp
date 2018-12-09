import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { BehaviorSubject } from 'rxjs';
import { JwtHelperService } from '@auth0/angular-jwt';
import { User } from '../_models/User';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthUser } from '../_models/authUser';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  baseUrl = environment.apiUrl;
  userToken: any;
  decodedToken: any;
  currentUser: User;
  // any to any communication
  private photoUrl = new BehaviorSubject<string>('../../assets/user.png');
  currentPhotoUrl = this.photoUrl.asObservable();

  constructor(private http: HttpClient, private jwtHelper: JwtHelperService) {}

  changeMemberPhoto(photoUrl: string) {
    this.photoUrl.next(photoUrl);
  }

  login(user: User) {
    // map our response to a user
    return this.http
      .post<AuthUser>(this.baseUrl + 'auth/login', user, {
        headers: new HttpHeaders().set('Content-Type', 'application/json')
      })
      .pipe(
        map(user => {
          if (user) {
            // setting token for a user i local storage
            localStorage.setItem('token', user.tokenString);
            localStorage.setItem('user', JSON.stringify(user.user));
            this.decodedToken = this.jwtHelper.decodeToken(user.tokenString);
            this.userToken = user.tokenString;
            this.currentUser = user.user;

            if (this.currentUser.photoUrl != null) {
              //we want to set the photourl of the current user to the photo url of the current user object
              this.changeMemberPhoto(this.currentUser.photoUrl);
            } else {
              this.changeMemberPhoto('../../assets/user.png');
            }
          }
        })
      );
  }

  loggedIn() {
    const token = this.jwtHelper.tokenGetter();

    if (!token) {
      return false;
    }
    // we check to see if the token is expired, returns false if not expired
    return !this.jwtHelper.isTokenExpired(token);
  }

  register(user: User) {
    return this.http.post(this.baseUrl + 'auth/register', user, {
      headers: new HttpHeaders().set('Content-Type', 'application/json')
    });
  }

  roleMatch(allowedRoles: string[]) {
    const userRole = this.decodedToken.role as Array<string>;
    let isMatch = false;
    allowedRoles.forEach(element => {
      if (userRole.includes(element)) {
        isMatch = true;
        return;
      }
    });
    return isMatch;
  }
}
