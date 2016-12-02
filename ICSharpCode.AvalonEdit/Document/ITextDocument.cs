using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace ICSharpCode.AvalonEdit.Document
{
    /// <summary>
    /// A document representing a source code file for refactoring.  Extends IDocument to include items in TextDocument
    /// </summary>
    public interface ITextDocument : IDocument, INotifyPropertyChanged
    {
        /// <summary>
		/// Gets the <see cref="UndoStack"/> of the document.
		/// </summary>
		/// <remarks>This property can also be used to set the undo stack, e.g. for sharing a common undo stack between multiple documents.</remarks>
        UndoStack UndoStack { get; }

        /// <summary>
		/// <para>Begins a group of document changes.</para>
		/// <para>Some events are suspended until EndUpdate is called, and the <see cref="UndoStack"/> will
		/// group all changes into a single action.</para>
		/// <para>Calling BeginUpdate several times increments a counter, only after the appropriate number
		/// of EndUpdate calls the events resume their work.</para>
		/// </summary>
		/// <remarks><inheritdoc cref="Changing"/></remarks>
        void BeginUpdate();

        /// <summary>
		/// Ends a group of document changes.
		/// </summary>
		/// <remarks><inheritdoc cref="Changing"/></remarks>
        void EndUpdate();

        /// <summary>
		/// Immediately calls <see cref="BeginUpdate()"/>,
		/// and returns an IDisposable that calls <see cref="EndUpdate()"/>.
		/// </summary>
		/// <remarks><inheritdoc cref="BeginUpdate"/></remarks>
        IDisposable RunUpdate();

        /// <summary>
		/// Occurs when a document change starts.
		/// </summary>
		/// <remarks><inheritdoc cref="Changing"/></remarks>
		event EventHandler UpdateStarted;

        /// <summary>
        /// Occurs when a document change is finished.
        /// </summary>
        /// <remarks><inheritdoc cref="Changing"/></remarks>
        event EventHandler UpdateFinished;

        /// <summary>
		/// Is raised before the document changes.
		/// </summary>
		/// <remarks>
		/// <para>Here is the order in which events are raised during a document update:</para>
		/// <list type="bullet">
		/// <item><description><b><see cref="BeginUpdate">BeginUpdate()</see></b></description>
		///   <list type="bullet">
		///   <item><description>Start of change group (on undo stack)</description></item>
		///   <item><description><see cref="UpdateStarted"/> event is raised</description></item>
		///   </list></item>
		/// <item><description><b><see cref="Insert(int,string)">Insert()</see> / <see cref="Remove(int,int)">Remove()</see> / <see cref="Replace(int,int,string)">Replace()</see></b></description>
		///   <list type="bullet">
		///   <item><description><see cref="Changing"/> event is raised</description></item>
		///   <item><description>The document is changed</description></item>
		///   <item><description><see cref="TextAnchor.Deleted">TextAnchor.Deleted</see> event is raised if anchors were
		///     in the deleted text portion</description></item>
		///   <item><description><see cref="Changed"/> event is raised</description></item>
		///   </list></item>
		/// <item><description><b><see cref="EndUpdate">EndUpdate()</see></b></description>
		///   <list type="bullet">
		///   <item><description><see cref="TextChanged"/> event is raised</description></item>
		///   <item><description><see cref="PropertyChanged"/> event is raised (for the Text, TextLength, LineCount properties, in that order)</description></item>
		///   <item><description>End of change group (on undo stack)</description></item>
		///   <item><description><see cref="UpdateFinished"/> event is raised</description></item>
		///   </list></item>
		/// </list>
		/// <para>
		/// If the insert/remove/replace methods are called without a call to <c>BeginUpdate()</c>,
		/// they will call <c>BeginUpdate()</c> and <c>EndUpdate()</c> to ensure no change happens outside of <c>UpdateStarted</c>/<c>UpdateFinished</c>.
		/// </para><para>
		/// There can be multiple document changes between the <c>BeginUpdate()</c> and <c>EndUpdate()</c> calls.
		/// In this case, the events associated with EndUpdate will be raised only once after the whole document update is done.
		/// </para><para>
		/// The <see cref="UndoStack"/> listens to the <c>UpdateStarted</c> and <c>UpdateFinished</c> events to group all changes into a single undo step.
		/// </para>
		/// </remarks>
		event EventHandler<DocumentChangeEventArgs> Changing;

        /// <summary>
		/// Is raised after the document has changed.
		/// </summary>
		/// <remarks><inheritdoc cref="Changing"/></remarks>
		event EventHandler<DocumentChangeEventArgs> Changed;

        /// <summary>
		/// This event is called after a group of changes is completed.
		/// </summary>
		/// <remarks><inheritdoc cref="Changing"/></remarks>
		new event EventHandler TextChanged;

        /// <summary>
		/// Gets a read-only list of lines.
		/// </summary>
		/// <remarks><inheritdoc cref="DocumentLine"/></remarks>
		IList<IDocumentLine> Lines { get; }

        /// <summary>
		/// Is raised when the LineCount property changes.
		/// </summary>
		[Obsolete("This event will be removed in a future version; use the PropertyChanged event instead")]
        event EventHandler LineCountChanged;

        /// <summary>
		/// Is raised when the TextLength property changes.
		/// </summary>
		/// <remarks><inheritdoc cref="Changing"/></remarks>
		[Obsolete("This event will be removed in a future version; use the PropertyChanged event instead")]
        event EventHandler TextLengthChanged;

        /// <summary>
		/// Gets the list of <see cref="ILineTracker"/>s attached to this document.
		/// You can add custom line trackers to this list.
		/// </summary>
		IList<ILineTracker> LineTrackers { get; }

        /// <summary>
		/// Gets/Sets the service provider associated with this document.
		/// By default, every TextDocument has its own ServiceContainer; and has the document itself
		/// registered as <see cref="IDocument"/> and <see cref="TextDocument"/>.
		/// </summary>
		IServiceProvider ServiceProvider { get; }

        /// <summary>
		/// Replaces text.
		/// </summary>
		/// <param name="offset">The starting offset of the text to be replaced.</param>
		/// <param name="length">The length of the text to be replaced.</param>
		/// <param name="text">The new text.</param>
		/// <param name="offsetChangeMappingType">The offsetChangeMappingType determines how offsets inside the old text are mapped to the new text.
		/// This affects how the anchors and segments inside the replaced region behave.</param>
		void Replace(int offset, int length, string text, OffsetChangeMappingType offsetChangeMappingType);

        /// <summary>
		/// Replaces text.
		/// </summary>
		void Replace(ISegment segment, string text);

        /// <summary>
		/// Removes text.
		/// </summary>
		void Remove(ISegment segment);

        /// <summary>
		/// Verifies that the current thread is the documents owner thread.
		/// Throws an <see cref="InvalidOperationException"/> if the wrong thread accesses the TextDocument.
		/// </summary>
		/// <remarks>
		/// <para>The TextDocument class is not thread-safe. A document instance expects to have a single owner thread
		/// and will throw an <see cref="InvalidOperationException"/> when accessed from another thread.
		/// It is possible to change the owner thread using the <see cref="SetOwnerThread"/> method.</para>
		/// </remarks>
		void VerifyAccess();

        /// <summary>
		/// Transfers ownership of the document to another thread. This method can be used to load
		/// a file into a TextDocument on a background thread and then transfer ownership to the UI thread
		/// for displaying the document.
		/// </summary>
		/// <remarks>
		/// <inheritdoc cref="VerifyAccess"/>
		/// <para>
		/// The owner can be set to null, which means that no thread can access the document. But, if the document
		/// has no owner thread, any thread may take ownership by calling <see cref="SetOwnerThread"/>.
		/// </para>
		/// </remarks>
		void SetOwnerThread(Thread newOwner);

        /// <summary>
		/// Gets if an update is running.
		/// </summary>
		/// <remarks><inheritdoc cref="BeginUpdate"/></remarks>
		bool IsInUpdate { get; }
    }
}
