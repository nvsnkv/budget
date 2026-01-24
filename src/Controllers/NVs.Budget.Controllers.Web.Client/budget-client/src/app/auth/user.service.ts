import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly user = new BehaviorSubject<User>({isAuthenticated: false});

  get current$() { return this.user.asObservable(); }

  constructor() { }

  setCurrentUser(user: User) { this.user.next(user); }
}

export interface User {
  isAuthenticated: boolean;

  id?: string;
  ownerInfo?: Owner;
}

export interface Owner {
  id: string;
  name: string;
}
