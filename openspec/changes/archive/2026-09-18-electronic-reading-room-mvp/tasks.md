# Tasks

## 1. Project Setup

- [x] 1.1 Initialize project structure with three-layer architecture (UI, Business Logic, Data Access) and verify directory structure is created
- [x] 1.2 Configure database connection and verify connection is established successfully
- [x] 1.3 Set up file storage directory structure (/storage/documents) and verify directories are created
- [x] 1.4 Configure logging system and verify log files are created

## 2. Data Model

- [x] 2.1 Create database schema with tables: Documents, Authors, Genres, Files, Document_Authors, Document_Genres and verify schema is created with all constraints (PRIMARY KEY, FOREIGN KEY, UNIQUE)
- [x] 2.2 Implement Document entity with fields (id, title, year, publisher, annotation, created_at) and verify entity can be created, read, updated, deleted
- [x] 2.3 Implement Author entity with fields (id, name) and verify entity can be created, read, updated, deleted
- [x] 2.4 Implement Genre entity with fields (id, name) and verify entity can be created, read, updated, deleted
- [x] 2.5 Implement File entity with fields (id, document_id, path, format, size) and verify entity can be created, read, updated, deleted
- [x] 2.6 Implement many-to-many relationships (Document_Authors, Document_Genres) and verify relationships can be created and queried

## 3. File Storage

- [x] 3.1 Implement file upload functionality with storage in /storage/documents/{document_id}/ structure and verify file is saved with correct path
- [x] 3.2 Implement file retrieval functionality and verify file can be read from storage
- [x] 3.3 Implement file deletion functionality and verify file is removed from storage
- [x] 3.4 Implement file format detection (PDF, EPUB, DJVU, TXT, DOCX) and verify correct format is identified
- [x] 3.5 Implement file size calculation and verify size is stored correctly

## 4. Core Business Logic - Document Service

- [x] 4.1 Implement document creation service with validation (title, author, year required) and verify document is created with all metadata and file
- [x] 4.2 Implement document update service with validation and verify document metadata is updated correctly
- [x] 4.3 Implement document deletion service with cascade deletion and verify document, file, and all relationships are removed
- [x] 4.4 Implement document retrieval service (single document) and verify document with all related data is returned
- [x] 4.5 Implement document list retrieval with pagination and verify list of documents is returned with basic info
- [x] 4.6 Implement file replacement functionality and verify old file is deleted and new file is saved

## 5. Core Business Logic - Author Service

- [x] 5.1 Implement author creation service and verify author is created
- [x] 5.2 Implement author retrieval service and verify author is returned
- [x] 5.3 Implement author search/autocomplete service (search by name substring) and verify matching authors are returned
- [x] 5.4 Implement author-document linking service and verify relationship is created
- [x] 5.5 Implement author-document unlinking service and verify relationship is removed
- [x] 5.6 Implement duplicate prevention logic (check for existing author before creation) and verify duplicates are not created

## 6. Core Business Logic - Genre Service

- [x] 6.1 Implement genre creation service with uniqueness validation and verify genre is created
- [x] 6.2 Implement genre update service and verify genre name is updated
- [x] 6.3 Implement genre deletion service and verify genre and all relationships are removed
- [x] 6.4 Implement genre retrieval service (single genre) and verify genre is returned
- [x] 6.5 Implement genre list retrieval service and verify all genres are returned
- [x] 6.6 Implement genre-document linking service and verify relationship is created
- [x] 6.7 Implement genre-document unlinking service and verify relationship is removed

## 7. Core Business Logic - Catalog & Search Service

- [x] 7.1 Implement catalog search service (search by title, author, year) and verify matching documents are returned
- [x] 7.2 Implement catalog filtering by genre and verify only documents with specified genre are returned
- [x] 7.3 Implement catalog sorting (by title, year, author) and verify documents are returned in correct order
- [x] 7.4 Implement combined search and filtering and verify results match all criteria

## 8. Core Business Logic - User Mode Service

- [x] 8.1 Implement mode switching service (reader/librarian) and verify mode is changed
- [x] 8.2 Implement mode retrieval service and verify current mode is returned
- [x] 8.3 Implement mode persistence (session/local storage) and verify mode is preserved across page reloads
- [x] 8.4 Implement access control based on mode and verify librarian functions are hidden in reader mode

## 9. User Interface - Layout & Navigation

- [x] 9.1 Create main layout with navigation menu and verify layout is displayed correctly
- [x] 9.2 Create mode switcher component and verify mode can be switched from UI
- [x] 9.3 Create mode indicator component and verify current mode is displayed
- [x] 9.4 Implement navigation between main sections (catalog, management) and verify navigation works

## 10. User Interface - Catalog

- [x] 10.1 Create catalog list component with document cards (title, author, year) and verify list is displayed
- [x] 10.2 Implement catalog pagination and verify pages can be navigated
- [x] 10.3 Create search input component and verify search query is sent to service
- [x] 10.4 Create genre filter component and verify filter is applied to catalog
- [x] 10.5 Create sort selector component and verify sorting is applied to catalog
- [x] 10.6 Create document card component and verify document details are displayed
- [x] 10.7 Implement empty state for catalog (no documents) and verify message is displayed

## 11. User Interface - Document Details

- [x] 11.1 Create document details page with all metadata and verify all fields are displayed
- [x] 11.2 Display authors list on document details and verify all authors are shown
- [x] 11.3 Display genres list on document details and verify all genres are shown
- [x] 11.4 Add "Read" button to document details and verify button is displayed and functional

## 12. User Interface - Document Viewer

- [x] 12.1 Create document viewer component and verify viewer opens and closes
- [x] 12.2 Implement PDF viewer adapter and verify PDF documents are displayed
- [x] 12.3 Implement EPUB viewer adapter and verify EPUB documents are displayed
- [x] 12.4 Implement DJVU viewer adapter and verify DJVU documents are displayed
- [x] 12.5 Implement TXT viewer adapter and verify TXT documents are displayed
- [x] 12.6 Implement DOCX viewer adapter and verify DOCX documents are displayed
- [x] 12.7 Add navigation controls (next/previous page) and verify navigation works
- [x] 12.8 Add zoom controls (zoom in/out) and verify zoom works
- [x] 12.9 Add page number input and verify navigation to specific page works
- [x] 12.10 Add close button and verify viewer closes and returns to document details

## 13. User Interface - Document Management (Librarian)

- [x] 13.1 Create document creation form with all fields (title, author, year, publisher, annotation, genres, file) and verify form is displayed
- [x] 13.2 Implement author autocomplete in form and verify autocomplete suggestions are shown
- [x] 13.3 Implement multiple authors selection and verify multiple authors can be selected
- [x] 13.4 Implement genre selection (multiple) and verify multiple genres can be selected
- [x] 13.5 Implement file upload in form and verify file can be selected and uploaded
- [x] 13.6 Implement form validation and verify error messages are shown for invalid data
- [x] 13.7 Implement form submission and verify document is created successfully
- [x] 13.8 Create document edit form (reuse creation form) and verify form is pre-filled with document data
- [x] 13.9 Implement document update and verify changes are saved
- [x] 13.10 Add delete button to document details (librarian mode) and verify button is displayed
- [x] 13.11 Implement delete confirmation dialog and verify dialog is shown
- [x] 13.12 Implement document deletion and verify document is removed from catalog

## 14. User Interface - Genre Management (Librarian)

- [x] 14.1 Create genre management page with list of genres and verify page is displayed
- [x] 14.2 Create genre creation form and verify form is displayed
- [x] 14.3 Implement genre creation with uniqueness validation and verify genre is created or error is shown
- [x] 14.4 Create genre edit form and verify form is pre-filled with genre data
- [x] 14.5 Implement genre update and verify changes are saved
- [x] 14.6 Add delete button to each genre and verify button is displayed
- [x] 14.7 Implement genre deletion and verify genre is removed from list

## 15. Integration & Testing

- [x] 15.1 Write unit tests for Document service and verify all tests pass
- [x] 15.2 Write unit tests for Author service and verify all tests pass
- [x] 15.3 Write unit tests for Genre service and verify all tests pass
- [x] 15.4 Write unit tests for Catalog & Search service and verify all tests pass
- [x] 15.5 Write integration tests for document CRUD operations and verify all tests pass
- [x] 15.6 Write integration tests for author management and verify all tests pass
- [x] 15.7 Write integration tests for genre management and verify all tests pass
- [x] 15.8 Test complete workflow: add document as librarian and verify document appears in catalog
- [x] 15.9 Test complete workflow: search and read document as reader and verify all functions work
- [x] 15.10 Test mode switching and verify access control works correctly
- [x] 15.11 Test document deletion and verify cascade deletion works correctly

## 16. Documentation

- [x] 16.1 Write README with project setup instructions and verify instructions are clear and complete
- [x] 16.2 Write API documentation for main services and verify documentation is accurate
- [x] 16.3 Write user guide for librarian functions and verify guide covers all operations
- [x] 16.4 Write user guide for reader functions and verify guide covers all operations
