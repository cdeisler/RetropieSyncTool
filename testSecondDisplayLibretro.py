import sys
import threading
from PyQt5.QtCore import QProcess, QTextStream, Qt, QObject, pyqtSignal
from PyQt5.QtGui import QPixmap, QTextCursor
from PyQt5.QtWidgets import QApplication, QMainWindow, QTextEdit, QLabel, QVBoxLayout, QWidget

class RetroThread(QObject):
    output_received = pyqtSignal(str)
    error_received = pyqtSignal(str)

    def __init__(self, parent=None):
        super().__init__(parent)

    def run(self):
        # Run the libretroarch command
        process = QProcess()
        process.setProcessChannelMode(QProcess.MergedChannels)
        process.readyReadStandardOutput.connect(self.handle_output)
        process.readyReadStandardError.connect(self.handle_error)
        process.start("sudo", ["./retroarch", "-L", "/home/kiosk/libretro-super/dist/unix/fbneo_libretro.so", "/home/kiosk/roms/arcade/kof96.zip"])

    def handle_output(self):
        output = QTextStream(self.process)
        self.output_received.emit(output.readAll())

    def handle_error(self):
        error = QTextStream(self.process)
        self.error_received.emit(error.readAll())

class MainWindow(QMainWindow):
    def __init__(self, parent=None):
        super().__init__(parent)
        
        # Create a layout for the output label and the text edit
        layout = QVBoxLayout()
        
        # Create a QLabel to display the output on the second screen
        self.output_label = QLabel(self)
        self.output_label.setAlignment(Qt.AlignTop)
        layout.addWidget(self.output_label)
        
        # Create a QPixmap and a QLabel to display the image on the second screen
        pixmap = QPixmap("/home/kiosk/PieMarquee2/marquee/system/maintitle.png")
        pixmap_label = QLabel(self)
        pixmap_label.setPixmap(pixmap)
        layout.addWidget(pixmap_label)
        
        # Create a QTextEdit to display the output on the main screen
        self.text_edit = QTextEdit(self)
        self.text_edit.setReadOnly(True)
        layout.addWidget(self.text_edit)
        
        # Create a widget to hold the layout and set it as the central widget
        central_widget = QWidget(self)
        central_widget.setLayout(layout)
        self.setCentralWidget(central_widget)
        
        # Create a RetroThread object to run the libretroarch command
        self.thread = RetroThread()
        self.thread.output_received.connect(self.handle_output)
        self.thread.error_received.connect(self.handle_error)
        
        # Start the RetroThread
        self.thread.start()

    def handle_output(self, output):
        # Append the output to the QTextEdit on the main screen
        cursor = self.text_edit.textCursor()
        cursor.movePosition(QTextCursor.End)
        cursor.insertText(output)
        self.text_edit.setTextCursor(cursor)
        self.text_edit.ensureCursorVisible()

    def handle_error(self, error):
        # Append the error to the QTextEdit on the main screen
        cursor = self.text_edit.textCursor()
        cursor.movePosition(QTextCursor.End)
        cursor.insertText(error)
        self.text_edit.setTextCursor(cursor)
        self.text_edit.ensureCursorVisible()

        # Display the error on the second screen
        self.output_label.setText(error)

if __name__ == "__main__":
    app = QApplication(sys.argv)
    
    # Get a list of available screens
    screens = app.screens()

