WinSleepWell - ���[�U�[�K�C�h

WinSleepWell�������p�����������肪�Ƃ��������܂��B���̃v���W�F�N�g�ɂ́AWindows���X���[�v���畜�A�����ۂɍēx�X���[�v��Ԃɂ���T�[�r�X�ƁA�}�E�X�̓����ɂ��X���[�v����̈Ӑ}���Ȃ����A���ŏ����ɗ}����^�X�N�o�[�풓�A�v���P�[�V�������܂܂�Ă��܂��B

## �Z�b�g�A�b�v�菇

1. **�_�E�����[�h**:
    - [�����[�X](https://github.com/isshiki/WinSleepWell/releases)�y�[�W����`WinSleepWell` ZIP�t�@�C�����_�E�����[�h���Ă��������B
    - **�Z�L�����e�B���̊m�F**:
        - �_�E�����[�h����ZIP�t�@�C�����E�N���b�N���A�u�v���p�e�B�v��I�����܂��B
        - �v���p�e�B�E�B���h�E�́u�S�ʁv�^�u���J���A��ʉ����ɂ���u�Z�L�����e�B�v�̍��ڂŁu������v�Ƀ`�F�b�N�������Ă��邩�m�F���܂��B
        - �u������v�̃I�v�V�������\������Ă���ꍇ�́A�`�F�b�N�����Ă���uOK�v���N���b�N���܂��B

2. **�z�u**:
    - ZIP�t�@�C�����𓀂���`WinSleepWell`�t�H���_�[��C�ӂ̃f�B���N�g���ɔz�u���Ă��������B

3. **PowerShell�̎��s�|���V�[�̐ݒ�** (�K�v�ȏꍇ):
    - �Ǘ��҂Ƃ��� **PowerShell** ���J���܂��B
    - ���̃R�}���h�����s���āA���݂̎��s�|���V�[���m�F���ۑ����܂�:
      ```powershell
      $OriginalPolicy = Get-ExecutionPolicy
      Write-Host "���݂̎��s�|���V�[: $OriginalPolicy"
      ```
    - ���s�|���V�[�� `Unrestricted` �ȊO�ł���ꍇ�́A���̃R�}���h�Őݒ��ύX���܂�:
      ```powershell
      Set-ExecutionPolicy Unrestricted -Scope CurrentUser
      ```
      **����:** ���s�|���V�[�� `Unrestricted` �ɐݒ肷��ƁA���ׂẴX�N���v�g�����s�\�ɂȂ�܂����A�C���^�[�l�b�g����_�E�����[�h���ꂽ�X�N���v�g�����s����ۂɌx�����\�������ꍇ������܂��B���s�|���V�[�̕ύX�ɂ͊Ǘ��Ҍ������K�v�ł��B���s�|���V�[��ύX���邱�ƂŐ�������ɂ��āA�v���W�F�N�g�̍쐬�҂͈�؂̐ӔC�𕉂��܂���B

4. **�Z�b�g�A�b�v�̎��s**:
    - ����PowerShell�E�B���h�E�ŁA`WinSleepWell`�t�H���_�[�Ɉړ����܂��B
    - ���̃R�}���h����͂��ăZ�b�g�A�b�v�����s���܂�:
      ```powershell
      .\setup.ps1 -i
      ```

5. **�A���C���X�g�[��**:
    - �T�[�r�X�ƃ^�X�N���A���C���X�g�[������ɂ́A���̃R�}���h�����s���܂�:
      ```powershell
      .\setup.ps1 -u
      ```

6. **���s�|���V�[�̃��Z�b�g (�ύX�����ꍇ)**:
    - �Z�b�g�A�b�v�܂��̓A���C���X�g�[��������������A�ȑO�Ɏ��s�|���V�[��ύX�����ꍇ�́A���̃R�}���h�����s���Č��̐ݒ�ɖ߂����Ƃ������߂��܂�:
      ```powershell
      Set-ExecutionPolicy $OriginalPolicy -Scope CurrentUser
      ```

## �Z�L�����e�B�Ɋւ��钍�ӎ���
PowerShell�̎��s�|���V�[��ύX����ƁA�V�X�e���̃Z�L�����e�B�ɉe����^����\��������܂��B���s�|���V�[��ύX����O�ɁA���̉e�����\���ɗ������A�Z�b�g�A�b�v������������͌��̐ݒ�ɖ߂����Ƃ������߂��܂��B���s�|���V�[�̕ύX�ɂ���Đ����邢���Ȃ����Z�L�����e�B���X�N�ɂ��Ă��A�v���W�F�N�g�̍쐬�҂͈�؂̐ӔC�𕉂��܂���B

## �g�p���@
�Z�b�g�A�b�v����������ƁA�ݒ��ʂ������I�ɕ\������܂��B�\������Ȃ��ꍇ�́A�^�X�N�o�[�ɏ풓���Ă���A�C�R�����_�u���N���b�N���Ă��������B�ݒ肪��������ƁA���̐ݒ���e�Ɋ�Â��āA�T�[�r�X��Windows�̃X���[�v��K�؂Ɉێ�����悤�ɂȂ�܂��B

**����:** �{�A�v���P�[�V��������уT�[�r�X�́A[ONEXPLAYER X1 AMD Edition](https://onexplayerstore.com/products/onexplayer-x1-amd-ryzen%E2%84%A2-7-8840u-10-95-3-in-1-gaming-handheld-pre-sale?variant=48759855481126)�p�ɓ��ʂɐ݌v����Ă��܂��B���̃f�o�C�X�ł����삷��\��������܂����A����ȓ����ۏ؂�����̂ł͂���܂���B�܂��A�{�A�v���P�[�V�����̎g�p�ɂ�萶�����̏�⎖�̂Ɋւ��āA�����͈�؂̐ӔC�𕉂����˂܂��̂ł��������������B

## �ǉ����

- **Issues�i�ۑ�j**: [https://github.com/isshiki/WinSleepWell/issues](https://github.com/isshiki/WinSleepWell/issues)
- **�\�[�X�R�[�h**: [https://github.com/isshiki/WinSleepWell](https://github.com/isshiki/WinSleepWell)

�o�O�̕񍐂�@�\�v�]������ꍇ�́A��L��Issues����񍐂ł��܂��B�������A�{���̋@�\�Ɋ܂܂�Ȃ����ʂȃ��[�X�P�[�X�ɑ΂���T�|�[�g�͈�؍s��Ȃ��\��ł��B�����̂��߂ɍ쐬�������̂��A�g�������l�����Ɍ��J��������������ł��B�������炸���������������B