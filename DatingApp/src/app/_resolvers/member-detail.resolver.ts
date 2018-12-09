import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { User } from '../_models/User';
import { Injectable } from '@angular/core';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class MemberDetailResolver implements Resolve<User>{

	constructor(private userService: UserService,
		private router: Router, private alertify: AlertifyService) { }

	// for getting parameter from the rul
	resolve(route: ActivatedRouteSnapshot): Observable<User> {
		return this.userService.getUser(route.params['id']).pipe(

			catchError(err => {
				this.alertify.error('Problem retrieving data');
				this.router.navigate(['/members']);
				return of(null);
			})
		);
	}

}

