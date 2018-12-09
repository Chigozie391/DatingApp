import { Component, OnInit, ViewChild } from '@angular/core';
import { User } from '../../_models/User';
import { ActivatedRoute } from '@angular/router';
import { AlertifyService } from '../../_services/alertify.service';
import { NgForm } from '@angular/forms';
import { UserService } from '../../_services/user.service';
import { AuthService } from '../../_services/auth.service';

@Component({
	selector: 'app-member-edit',
	templateUrl: './member-edit.component.html',
	styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
	user: User;
	photoUrl: string;
	// to access the form
	@ViewChild('editForm') editForm: NgForm

	constructor(private route: ActivatedRoute,
		private alertify: AlertifyService, private userService: UserService,
		private authService: AuthService) { }

	ngOnInit() {
		this.route.data.subscribe(data => {
			// gets the user details pass from the resolver
			this.user = data['user'];
		});

		this.authService.currentPhotoUrl.subscribe(photoUrl => this.photoUrl = photoUrl);

	}
	updateUser() {
		this.userService.updateUser(this.authService.decodedToken.nameid, this.user)
			.subscribe(next => {
				this.alertify.success("Profile updated successfully");
				// reset the form so control o back to initial state
				this.editForm.reset(this.user);
			}, error => {
				this.alertify.error(error);
			});
	}
	updateMainPhoto(photoUrl) {
		// step 4 change the picture
		this.user.photoUrl = photoUrl;
	}

}
