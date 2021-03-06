import { Injectable } from '@angular/core';
import { Author } from '../../../model/author';
import { Observable } from 'rxjs/Observable';
import { AuthorService } from '../../../service/author.service';
import { SuggestionAdapter } from '../typeahead/suggestion.adapter';

@Injectable()
export class AuthorSuggestionAdapter implements SuggestionAdapter {
    constructor(private service: AuthorService) {
    }

    public search(query: string): Observable<Author[]> {
        return this.service.search(query);
    }
}
