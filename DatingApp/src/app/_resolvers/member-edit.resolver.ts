import { Resolve, Router } from '@angular/router';
import { User } from '../_models/User';
import { Injectable } from '@angular/core';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../_services/auth.service';

@Injectable()
export class MemberEditResolver implements Resolve<User>{

	constructor(private userService: UserService,
		private router: Router, private alertify: AlertifyService, private authService: AuthService) { }

	// for getting parameter from the url
	resolve(): Observable<User> {
		//gets the particular user
		return this.userService.getUser(this.authService.decodedToken.nameid).pipe(

			catchError(err => {
				this.alertify.error('Problem retrieving data');
				this.router.navigate(['/members']);
				return of(null);
			})
		);
	}

}

