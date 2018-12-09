import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse, HTTP_INTERCEPTORS } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		return next.handle(req).pipe(
			catchError(error => {
				if (error instanceof HttpErrorResponse) {

					// internal server error 500
					const applicationError = error.headers.get('Application-Error');
					if (applicationError) {
						return throwError(applicationError);
					}
					// store as json for reference with key
					const serverError = error.error;
					let modelStateError = '';
					if (serverError && serverError === 'object') {
						for (const key in serverError) {
							// get the message in the key
							if (serverError[key]) {
								modelStateError += serverError[key] + '\n';
							}
						}
					}
					return throwError(
						modelStateError || serverError || 'Server Error'
					);
				}

			})
		)
	}

}
export const ErrorInterceptorProvider = {
	provide: HTTP_INTERCEPTORS,
	useClass: ErrorInterceptor,
	// prevent overriding our current htto interceptors
	multi: true
}