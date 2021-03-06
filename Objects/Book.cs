using System.Collections.Generic;
using System.Data.SqlClient;
using System;

namespace Library
{
    public class Book
    {
        private int _id;
        private string _title;

        public Book(string Title, int Id = 0)
        {
            _id = Id;
            _title = Title;
        }

        public int GetId()
        {
            return _id;
        }

        public string GetTitle()
        {
            return _title;
        }

        public override bool Equals(System.Object otherBook)
        {
            if(!(otherBook is Book))
            {
                return false;
            }
            else
            {
                Book newBook = (Book) otherBook;
                bool idEquality = (this.GetId() == newBook.GetId());
                bool titleEquality = (this.GetTitle() == newBook.GetTitle());
                return (idEquality && titleEquality);
            }
        }

        public override int GetHashCode()
        {
            return this.GetTitle().GetHashCode();
        }

        public static List<Book> GetAll()
        {
            List<Book> AllBooks = new List<Book> {};

            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM books;", conn);
            SqlDataReader rdr = cmd.ExecuteReader();

            while(rdr.Read())
            {
                int bookId = rdr.GetInt32(0);
                string bookTitle = rdr.GetString(1);

                Book newBook = new Book(bookTitle, bookId);
                AllBooks.Add(newBook);
            }

            if(rdr != null)
            {
                rdr.Close();
            }
            if(conn != null)
            {
                conn.Close();
            }
            return AllBooks;
        }

        public void Save()
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("INSERT INTO books(title) OUTPUT INSERTED.id VALUES(@BookTitle);", conn);

            SqlParameter bookTitle = new SqlParameter();
            bookTitle.ParameterName = "@BookTitle";
            bookTitle.Value = this.GetTitle();
            cmd.Parameters.Add(bookTitle);

            SqlDataReader rdr = cmd.ExecuteReader();

            while(rdr.Read())
            {
                this._id = rdr.GetInt32(0);
            }

            if(conn != null)
            {
                conn.Close();
            }
        }

        public static Book FindById(int id)
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM books WHERE id = @BookId;", conn);

            SqlParameter bookIdParameter = new SqlParameter();
            bookIdParameter.ParameterName = "@BookId";
            bookIdParameter.Value = id.ToString();
            cmd.Parameters.Add(bookIdParameter);

            SqlDataReader rdr = cmd.ExecuteReader();

            int foundBookId = 0;
            string foundBookTitle = null;

            while(rdr.Read())
            {
                foundBookId = rdr.GetInt32(0);
                foundBookTitle = rdr.GetString(1);
            }
            Book foundBook = new Book(foundBookTitle, foundBookId);

            if(rdr != null)
            {
                rdr.Close();
            }
            if (conn != null)
            {
                conn.Close();
            }
            return foundBook;
        }

        public void Update(string newTitle)
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("UPDATE books SET title= @NewBookTitle OUTPUT INSERTED.title WHERE id = @BookId;", conn);

            SqlParameter newBookTitle = new SqlParameter();
            newBookTitle.ParameterName = "@NewBookTitle";
            newBookTitle.Value = newTitle;
            cmd.Parameters.Add(newBookTitle);

            SqlParameter bookId = new SqlParameter();
            bookId.ParameterName = "@BookId";
            bookId.Value = this.GetId();
            cmd.Parameters.Add(bookId);

            SqlDataReader rdr = cmd.ExecuteReader();

            while(rdr.Read())
            {
                this._title = rdr.GetString(0);
            }
            if(rdr != null)
            {
                rdr.Close();
            }
            if (conn != null)
            {
                conn.Close();
            }
        }

        public void AddAuthor(Author newAuthor)
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("INSERT INTO authors_books(author_id, book_id) VALUES (@AuthorId, @BookId);", conn);

            SqlParameter authorIdParameter = new SqlParameter();
            authorIdParameter.ParameterName = "@AuthorId";
            authorIdParameter.Value = newAuthor.GetId();
            cmd.Parameters.Add(authorIdParameter);

            SqlParameter bookIdParameter = new SqlParameter();
            bookIdParameter.ParameterName = "@BookId";
            bookIdParameter.Value = this.GetId();
            cmd.Parameters.Add(bookIdParameter);

            cmd.ExecuteNonQuery();

            if(conn != null)
            {
                conn.Close();
            }
        }

        public List<Author> GetAuthors()
        {
            List<Author> AllAuthors = new List<Author>{};

            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT authors.* FROM books JOIN authors_books ON (books.id = authors_books.book_id) JOIN authors ON (authors.id = authors_books.author_id) WHERE book_id = @BookId;", conn);

            SqlParameter bookIdParameter = new SqlParameter();
            bookIdParameter.ParameterName = "@BookId";
            bookIdParameter.Value = this.GetId().ToString();
            cmd.Parameters.Add(bookIdParameter);

            SqlDataReader rdr = cmd.ExecuteReader();

            while(rdr.Read())
            {
                int authorId = rdr.GetInt32(0);
                string authorName = rdr.GetString(1);

                Author newAuthor = new Author(authorName, authorId);
                AllAuthors.Add(newAuthor);
            }

            if(rdr != null)
            {
                rdr.Close();
            }
            if(conn != null)
            {
                conn.Close();
            }

            return AllAuthors;
        }

        public List<Copy> GetCopies()
        {
            List<Copy> AllCopies = new List<Copy>{};
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM copies WHERE book_id = @BookId;", conn);
            SqlParameter bookIdParameter = new SqlParameter();
            bookIdParameter.ParameterName = "@BookId";
            bookIdParameter.Value = this.GetId();
            cmd.Parameters.Add(bookIdParameter);

            SqlDataReader rdr = cmd.ExecuteReader();
            while(rdr.Read())
            {
                int id = rdr.GetInt32(0);
                int bookId = rdr.GetInt32(1);
                bool copyAvailability;
                if (rdr.GetByte(2) == 1)
                {
                    copyAvailability = true;
                }
                else
                {
                    copyAvailability = false;
                }

                Copy newCopy = new Copy(bookId, id);
                newCopy.SetAvailability(copyAvailability);
                AllCopies.Add(newCopy);
            }

            if(rdr != null)
            {
                rdr.Close();
            }

            if(conn != null)
            {
                conn.Close();
            }

            return AllCopies;
        }

        public void Delete()
        {
            SqlConnection conn = DB.Connection();
            conn.Open();

            SqlCommand cmd = new SqlCommand("DELETE FROM books WHERE id = @BookId;", conn);

            SqlParameter bookIdParameter = new SqlParameter();
            bookIdParameter.ParameterName = "@BookId";
            bookIdParameter.Value = this.GetId();
            cmd.Parameters.Add(bookIdParameter);

            cmd.ExecuteNonQuery();

            if(conn != null)
            {
                conn.Close();
            }
        }

        public static void DeleteAll()
        {
            SqlConnection conn = DB.Connection();
            conn.Open();
            SqlCommand cmd = new SqlCommand("DELETE FROM books;", conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}
