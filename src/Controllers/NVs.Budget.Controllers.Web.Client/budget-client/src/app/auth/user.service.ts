import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly userSignal = signal<User>({isAuthenticated: false});

  readonly currentUser = this.userSignal.asReadonly();

  setCurrentUser(user: User) { this.userSignal.set(user); }
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
