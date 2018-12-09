import { Component, OnInit } from '@angular/core';
import { User } from '../_models/User';
import { Pagination, PaginatedResult } from '../_models/pagination';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { ActivatedRoute } from '@angular/router';

@Component({
	selector: 'app-lists',
	templateUrl: './lists.component.html',
	styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
	users: User[];
	pagination: Pagination;
	likesParams: string;

	constructor(private userService: UserService, private alertify: AlertifyService,
		private route: ActivatedRoute) { }

	ngOnInit() {
		this.route.data.subscribe(data => {
			this.users = data['users'].result;
			this.pagination = data['users'].pagination;
		});
		this.likesParams = 'likers';
	}

	loadUsers() {
		this.userService.getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, null, this.likesParams)
			.subscribe((res: PaginatedResult<User[]>) => {
				this.users = res.result
				this.pagination = res.pagination;
			}, error => {
				this.alertify.error(error);
			})
	}

	pageChanged(event: any): void {
		this.pagination.currentPage = event.page;
		this.loadUsers();
	}

}
