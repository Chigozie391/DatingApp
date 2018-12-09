import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { Injectable } from '@angular/core';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Message } from '../_models/message';
import { AuthService } from '../_services/auth.service';

@Injectable()
export class MessageResolver implements Resolve<Message[]>{
	pageSize = 5;
	pageNumber = 1;
	messageContainer = 'Unread';

	constructor(private userService: UserService,
		private router: Router, private alertify: AlertifyService, private authService: AuthService) { }

	// for getting parameter from the rul
	resolve(route: ActivatedRouteSnapshot): Observable<Message[]> {

		return this.userService.getMessage(this.authService.decodedToken.nameid, this.pageNumber,
			this.pageSize, this.messageContainer).pipe(

				catchError(err => {
					this.alertify.error('Problem retrieving data');
					this.router.navigate(['/home']);
					return of(null);
				})

			);
	}

}

